using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Nghex.Data;
using Nghex.Data.Factory;
using Nghex.Data.Factory.Interfaces;
using Nghex.Data.Setup;

namespace Nghex.Data.Extensions
{
    public static class NghexDataServiceCollectionExtensions
    {
        /// <summary>
        /// Registers core Nghex.Data services (connection, executor, factories) and database bootstrap (<see cref="IDatabaseSetupService"/>).
        /// Also initializes Dapper.FluentMap one time from <c>[Column]</c> attributes so Dapper binds columns (and aliases)
        /// strictly by what entities declare, with no implicit naming heuristics.
        /// </summary>
        /// <param name="services">The DI container.</param>
        /// <param name="entityAssemblies">
        /// Optional explicit assemblies to scan for <c>[Column]</c>-attributed types. When omitted, all currently loaded
        /// non-framework assemblies in the current <see cref="AppDomain"/> are scanned. Pass explicit assemblies when
        /// some entity assemblies are lazily loaded (plugins, pay-per-tenant modules, …).
        /// </param>
        public static IServiceCollection AddNghexDataInfrastructure(
            this IServiceCollection services,
            params Assembly[] entityAssemblies)
        {
            services.AddSingleton<IDatabaseConnection, DatabaseConnection>();
            services.AddSingleton<IDatabaseProviderFactory, DatabaseProviderFactory>();
            services.AddSingleton<IDatabaseExecutorHelperFactory, DatabaseExecutorHelperFactory>();
            services.AddScoped<IDatabaseExecutor, DatabaseExecutor>();
            services.AddScoped<IDatabaseSetupService, DatabaseSetupService>();
            services.AddSingleton<ISystemInitializationState, SystemInitializationState>();

            // Dapper.FluentMap.Initialize can only run once per process; we do it here so callers never forget.
            // If assemblies are not provided, fall back to the currently loaded, non-framework assemblies.
            var assembliesToScan = entityAssemblies.Length > 0
                ? entityAssemblies
                : DiscoverCandidateAssemblies();
            DapperAutoMapping.RegisterAllMappings(assembliesToScan);

            return services;
        }

        /// <summary>
        /// Returns currently loaded assemblies that are reasonable candidates to contain <c>[Column]</c>-annotated entities
        /// (i.e. not BCL, Microsoft.*, Dapper.*, etc.). Scanning non-candidates is harmless but wastes startup time.
        /// </summary>
        private static Assembly[] DiscoverCandidateAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                    !a.IsDynamic &&
                    !string.IsNullOrEmpty(a.FullName) &&
                    !IsFrameworkAssembly(a.FullName!))
                .ToArray();
        }

        private static bool IsFrameworkAssembly(string fullName)
        {
            return fullName.StartsWith("System.", StringComparison.Ordinal)
                || fullName.StartsWith("System,", StringComparison.Ordinal)
                || fullName.StartsWith("Microsoft.", StringComparison.Ordinal)
                || fullName.StartsWith("netstandard,", StringComparison.Ordinal)
                || fullName.StartsWith("mscorlib,", StringComparison.Ordinal)
                || fullName.StartsWith("Dapper,", StringComparison.Ordinal)
                || fullName.StartsWith("Dapper.", StringComparison.Ordinal)
                || fullName.StartsWith("Mapster,", StringComparison.Ordinal)
                || fullName.StartsWith("Mapster.", StringComparison.Ordinal)
                || fullName.StartsWith("AutoMapper,", StringComparison.Ordinal)
                || fullName.StartsWith("AutoMapper.", StringComparison.Ordinal)
                || fullName.StartsWith("Serilog", StringComparison.Ordinal)
                || fullName.StartsWith("Newtonsoft.", StringComparison.Ordinal)
                || fullName.StartsWith("Swashbuckle.", StringComparison.Ordinal);
        }
    }
}
