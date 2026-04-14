using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Nghex.Core.Mapping;

/// <summary>
/// Mapster configuration for Nghex.Core (Configuration + Plugin domains).
/// Identity domain mappings are registered via Nghex.Identity.Mapping.IdentityMapsterConfig.
/// </summary>
public static class MapsterConfig
{
    /// <summary>
    /// Add Mapster configuration to DI container (Core domains only).
    /// Call Nghex.Identity.Mapping.IdentityMapsterConfig.RegisterMappings() separately if using Identity.
    /// </summary>
    public static IServiceCollection AddMapsterConfiguration(this IServiceCollection services)
    {
        RegisterMappings();

        var config = TypeAdapterConfig.GlobalSettings;
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    public static void RegisterMappings()
    {
        // Plugin mappings moved to Nghex.Plugins.Mapping.PluginMappingConfig
        // ConfigurationMappingConfig moved to Nghex.Configuration — call separately if using that package
    }
}
