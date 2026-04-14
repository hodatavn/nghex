using System.Reflection;
using System.Linq.Expressions;
using Dapper.FluentMap;
using Dapper.FluentMap.Configuration;
using Dapper.FluentMap.Mapping;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Data;
public static class DapperAutoMapping
{ 
    private static bool _isInitialized = false;
    public static void RegisterAllMappings(params Assembly[] assemblies)
    {
        // Dapper.FluentMap only allows Initialize() to be called once
        if (_isInitialized) return;

        FluentMapper.Initialize(config =>
        {
            foreach (var asm in assemblies)
            {
                var modelTypes = asm.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract &&
                                t.GetProperties().Any(p => p.GetCustomAttribute<ColumnAttribute>() != null));

                foreach (var modelType in modelTypes)
                {
                    var mapType = typeof(DynamicEntityMap<>).MakeGenericType(modelType);
                    var mapInstance = Activator.CreateInstance(mapType)!;

                    // Call AddMap<TEntity>(IEntityMap<TEntity>) by reflection
                    var addMapMethod = typeof(FluentMapConfiguration)
                        .GetMethods()
                        .First(m => m.Name == "AddMap" && m.IsGenericMethod);

                    var genericAddMap = addMapMethod.MakeGenericMethod(modelType);
                    genericAddMap.Invoke(config, [mapInstance]);
                }
            }
        });

        _isInitialized = true;
    }

    private class DynamicEntityMap<T> : EntityMap<T> where T : class
    {
        public DynamicEntityMap()
        {
            try
            {
                foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var column = prop.GetCustomAttribute<ColumnAttribute>();
                    if (column != null)
                    {
                        try
                        {
                            // Create parameter expression: x => 
                            var parameter = Expression.Parameter(typeof(T), "x");
                            
                            // Create property access expression: x.PropertyName
                            var propertyAccess = Expression.Property(parameter, prop);
                            
                            // Convert to object if needed: (object)x.PropertyName
                            var convertedProperty = Expression.Convert(propertyAccess, typeof(object));
                            
                            // Create lambda expression: x => (object)x.PropertyName
                            var lambda = Expression.Lambda<Func<T, object>>(convertedProperty, parameter);
                            
                            // Call Map with the lambda expression
                            Map(lambda).ToColumn(column.Name);
                            // Console.WriteLine($"Registered Dapper mapping for {typeof(T).Name}.{prop.Name} to {column.Name}");
                        }
                        catch (Exception propEx)
                        {
                            Console.WriteLine($"Failed to register mapping for {typeof(T).Name}.{prop.Name}: {propEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register Dapper mappings for {typeof(T).Name}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
