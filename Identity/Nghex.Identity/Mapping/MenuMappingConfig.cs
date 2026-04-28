using Mapster;
using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Mapping;

public static class MenuMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<MenuItemEntity, MenuItemResponse>.NewConfig()
            .Map(dest => dest.MenuId, src => src.Id);

        TypeAdapterConfig<CreateMenuItemRequest, MenuItemEntity>.NewConfig()
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
            .Ignore(dest => dest.UpdatedBy!);

        TypeAdapterConfig<UpdateMenuItemRequest, MenuItemEntity>.NewConfig()
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
            .Ignore(dest => dest.UpdatedAt!);
    }
}
