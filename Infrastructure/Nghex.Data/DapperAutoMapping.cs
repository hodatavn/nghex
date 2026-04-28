using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Dapper;

namespace Nghex.Data;

/// <summary>
/// Registers Dapper <see cref="SqlMapper.ITypeMap"/>s so SQL columns bind strictly to the
/// <c>[Column]</c> attribute declared on entity properties. Works for derived row types (e.g.
/// anonymous/private types extending a persistence entity) because mapping is per-type and
/// honored through the full property chain.
/// </summary>
/// <remarks>
/// Unlike <c>Dapper.FluentMap.FluentMapper.Initialize</c> (one-time, whole-process), this uses
/// <see cref="SqlMapper.SetTypeMap"/> directly, which is safe to call repeatedly and per type.
/// Also hooks <see cref="AppDomain.AssemblyLoad"/> so types loaded later (plugins, lazy-loaded
/// modules) get registered automatically.
/// </remarks>
public static class DapperAutoMapping
{
    private static readonly ConcurrentDictionary<Type, byte> _registered = new();
    private static readonly object _hookLock = new();
    private static bool _assemblyLoadHookInstalled;

    /// <summary>
    /// Scans each assembly for types exposing <c>[Column]</c> and registers an attribute-aware
    /// Dapper type map. Safe to call multiple times and with disjoint assembly sets.
    /// </summary>
    public static void RegisterAllMappings(params Assembly[] assemblies)
    {
        EnsureAssemblyLoadHook();

        if (assemblies is null) return;
        foreach (var asm in assemblies)
        {
            if (asm is null) continue;
            RegisterAssembly(asm);
        }
    }

    /// <summary>
    /// Registers a single type's Dapper map (idempotent). Returns <c>true</c> when a map was
    /// actually added (the type has at least one <c>[Column]</c> property), <c>false</c> otherwise.
    /// </summary>
    public static bool RegisterType(Type type)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));
        if (!type.IsClass || type.IsAbstract) return false;
        if (!_registered.TryAdd(type, 0)) return false;

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var hasColumnAttr = properties.Any(p => p.GetCustomAttribute<ColumnAttribute>() != null);
        if (!hasColumnAttr) return false;

        try
        {
            SqlMapper.SetTypeMap(type, new ColumnAttributeTypeMap(type));
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to register Dapper map for {type.FullName}: {ex.Message}");
            return false;
        }
    }

    private static void RegisterAssembly(Assembly asm)
    {
        Type?[] types;
        try
        {
            types = asm.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Partial load (e.g. a referenced assembly missing at runtime): proceed with what loaded.
            types = ex.Types;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Skipping assembly {asm.FullName} during Dapper mapping scan: {ex.Message}");
            return;
        }

        foreach (var t in types)
        {
            if (t is null) continue;
            RegisterType(t);
        }
    }

    /// <summary>
    /// Subscribes to <see cref="AppDomain.AssemblyLoad"/> once so future assembly loads
    /// (plugins, reflection-loaded modules) also get their <c>[Column]</c> maps registered.
    /// </summary>
    private static void EnsureAssemblyLoadHook()
    {
        if (_assemblyLoadHookInstalled) return;
        lock (_hookLock)
        {
            if (_assemblyLoadHookInstalled) return;
            AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
            {
                var asm = args.LoadedAssembly;
                // Skip system/framework/third-party assemblies: they never use [Column] and
                // scanning them across AssemblyLoadContexts causes TypeLoadExceptions because
                // Dapper (which ColumnAttributeTypeMap depends on) may not be loaded in the
                // originating AssemblyLoadContext (e.g. a plugin's isolated ALC).
                if (IsFrameworkOrThirdPartyAssembly(asm)) return;
                try { RegisterAssembly(asm); }
                catch (Exception ex) { Console.WriteLine($"AssemblyLoad hook failed for {asm.FullName}: {ex.Message}"); }
            };
            _assemblyLoadHookInstalled = true;
        }
    }

    private static bool IsFrameworkOrThirdPartyAssembly(Assembly asm)
    {
        var name = asm.GetName().Name;
        if (name is null) return true;
        return name.StartsWith("System.", StringComparison.Ordinal)
            || name.StartsWith("Microsoft.", StringComparison.Ordinal)
            || name.StartsWith("Newtonsoft.", StringComparison.Ordinal)
            || name is "System" or "mscorlib" or "netstandard" or "Dapper"
            || asm.IsDynamic;
    }

    /// <summary>
    /// <see cref="SqlMapper.ITypeMap"/> that resolves SQL column names via <c>[Column]</c> first,
    /// then falls back to <see cref="DefaultTypeMap"/> (property-name match) for columns without
    /// an attribute mapping (e.g. computed projections like <c>1 AS IsAccessible</c>).
    /// </summary>
    // Kept public for binary compatibility with previously compiled assemblies/plugins
    // that may reference Nghex.Data.DapperAutoMapping+ColumnAttributeTypeMap directly.
    public sealed class ColumnAttributeTypeMap : SqlMapper.ITypeMap
    {
        private readonly Type _type;
        private readonly DefaultTypeMap _fallback;
        private readonly Dictionary<string, PropertyInfo> _columnToProperty;

        public ColumnAttributeTypeMap(Type type)
        {
            _type = type;
            _fallback = new DefaultTypeMap(type);
            _columnToProperty = BuildColumnIndex(type);
        }

        private static Dictionary<string, PropertyInfo> BuildColumnIndex(Type type)
        {
            var dict = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var col = p.GetCustomAttribute<ColumnAttribute>();
                if (col?.Name is { Length: > 0 } name && !dict.ContainsKey(name))
                    dict[name] = p;
            }
            return dict;
        }

        public ConstructorInfo? FindConstructor(string[] names, Type[] types)
            => _fallback.FindConstructor(names, types);

        public ConstructorInfo? FindExplicitConstructor()
            => _fallback.FindExplicitConstructor();

        public SqlMapper.IMemberMap? GetConstructorParameter(ConstructorInfo constructor, string columnName)
            => _fallback.GetConstructorParameter(constructor, columnName);

        public SqlMapper.IMemberMap? GetMember(string columnName)
        {
            if (_columnToProperty.TryGetValue(columnName, out var prop))
            {
                // Delegate to DefaultTypeMap so Dapper builds a normal SimpleMemberMap for this property.
                return _fallback.GetMember(prop.Name);
            }
            return _fallback.GetMember(columnName);
        }
    }
}
