using Mapster;
using Nghex.Plugins.DTOs;
using Nghex.Plugins.Persistence.Entities;

namespace Nghex.Plugins.Mapping;

/// <summary>
/// Mapster mapping configuration for Plugin domain
/// </summary>
public static class PluginMappingConfig
{
    public static void Configure()
    {
        // Entity -> DTO
        TypeAdapterConfig<PluginEntity, PluginDto>.NewConfig();
    }
}
