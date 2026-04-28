using Mapster;
using Nghex.Plugins.Models;
using Nghex.Plugins.Persistence.Entities;

namespace Nghex.Plugins.Mapping;

public static class PluginMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<PluginEntity, PluginResponseModel>.NewConfig();
    }
}
