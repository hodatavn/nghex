using Mapster;
using Nghex.Identity.DTOs.Menus;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Mapping;

/// <summary>
/// Mapster mapping configuration for Menu domain
/// </summary>
public static class MenuMappingConfig
{
    /// <summary>
    /// Configure Menu mappings
    /// </summary>
    public static void Configure()
    {
        // Entity -> DTO
        TypeAdapterConfig<MenuItemEntity, MenuItemDto>.NewConfig()
            .Ignore(dest => dest.Children!);

        // CreateDto -> Entity
        TypeAdapterConfig<CreateMenuItemDto, MenuItemEntity>.NewConfig()
            .Map(dest => dest.MenuKey, src => src.MenuKey)
            .Map(dest => dest.ParentKey, src => src.ParentKey)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Route, src => src.Route)
            .Map(dest => dest.Icon, src => src.Icon)
            .Map(dest => dest.PluginName, src => src.PluginName)
            .Map(dest => dest.PermissionPrefix, src => src.PermissionPrefix)
            .Map(dest => dest.SortOrder, src => src.SortOrder)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.CreatedBy, src => src.CreatedBy)
            .Ignore(dest => dest.Id!)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.UpdatedBy!)
            .Ignore(dest => dest.PluginName!);

        // UpdateDto -> Entity
        TypeAdapterConfig<UpdateMenuItemDto, MenuItemEntity>.NewConfig()
            .Map(dest => dest.MenuKey, src => src.MenuKey)
            .Map(dest => dest.ParentKey, src => src.ParentKey)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Route, src => src.Route)
            .Map(dest => dest.Icon, src => src.Icon)
            .Map(dest => dest.PluginName, src => src.PluginName)
            .Map(dest => dest.PermissionPrefix, src => src.PermissionPrefix)
            .Map(dest => dest.SortOrder, src => src.SortOrder)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.UpdatedBy, src => src.UpdatedBy)
            .Ignore(dest => dest.Id!)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.CreatedBy!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.PluginName!);
    }
}
