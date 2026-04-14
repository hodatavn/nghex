using Mapster;
using Nghex.Identity.DTOs.Roles;
using Nghex.Identity.Enum;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Mapping;

/// <summary>
/// Mapster mapping configuration for Role domain
/// </summary>
public static class RoleMappingConfig
{
    /// <summary>
    /// Configure Role mappings
    /// </summary>
    public static void Configure()
    {
        // Entity -> DTO
        TypeAdapterConfig<RoleEntity, RoleDto>.NewConfig();

        // CreateDto -> Entity
        TypeAdapterConfig<CreateRoleDto, RoleEntity>.NewConfig()
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

        // UpdateDto -> Entity
        TypeAdapterConfig<UpdateRoleDto, RoleEntity>.NewConfig()
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
