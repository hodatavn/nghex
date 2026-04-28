using Mapster;
using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Identity.Enum;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Mapping;

public static class RoleMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<RoleEntity, RoleResponse>.NewConfig()
            .Map(dest => dest.RoleId, src => src.Id)
            .Map(dest => dest.RoleCode, src => src.Code)
            .Map(dest => dest.RoleName, src => src.Name);

        TypeAdapterConfig<CreateRoleRequest, RoleEntity>.NewConfig()
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.RoleLevel, src => src.RoleLevel.FromLevel())
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.CreatedBy, src => src.CreatedBy)
            .Ignore(dest => dest.Id!)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.UpdatedBy!)
            .Ignore(dest => dest.IsDeleted!);

        TypeAdapterConfig<UpdateRoleRequest, RoleEntity>.NewConfig()
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.RoleLevel, src => src.RoleLevel.FromLevel())
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.UpdatedBy, src => src.UpdatedBy)
            .Ignore(dest => dest.Id!)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.CreatedBy!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.IsDeleted!);
    }
}
