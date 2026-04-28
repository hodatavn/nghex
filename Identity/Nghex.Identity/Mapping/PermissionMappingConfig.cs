using Mapster;
using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Mapping;

public static class PermissionMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<PermissionEntity, PermissionResponse>.NewConfig();

        TypeAdapterConfig<CreatePermissionRequest, PermissionEntity>.NewConfig()
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Module, src => src.Module)
            .Map(dest => dest.PluginName, src => src.PluginName)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.CreatedBy, src => src.CreatedBy)
            .Ignore(dest => dest.Id!)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.UpdatedBy!)
            .Ignore(dest => dest.IsDeleted!);

        TypeAdapterConfig<UpdatePermissionRequest, PermissionEntity>.NewConfig()
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.PluginName, src => src.PluginName)
            .Map(dest => dest.Module, src => src.Module)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.UpdatedBy, src => src.UpdatedBy)
            .Ignore(dest => dest.Id!)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.CreatedBy!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.IsDeleted!);
    }
}
