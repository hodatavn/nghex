using Microsoft.Extensions.DependencyInjection;

namespace Nghex.Configuration.Mapping;

public static class ConfigurationMapsterConfig
{
    public static IServiceCollection AddConfigurationMappings(this IServiceCollection services)
    {
        RegisterMappings();
        return services;
    }

    public static void RegisterMappings()
    {
        ConfigurationMappingConfig.Configure();
    }
}
