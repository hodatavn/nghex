using Microsoft.Extensions.DependencyInjection;

namespace Nghex.Identity.Mapping;

/// <summary>
/// Mapster configuration for Nghex.Identity (Account, Role, Permission, Menu domains).
/// Call AddIdentityMappings() after AddMapsterConfiguration() from Nghex.Core.
/// </summary>
public static class IdentityMapsterConfig
{
    public static IServiceCollection AddIdentityMappings(this IServiceCollection services)
    {
        RegisterMappings();
        return services;
    }

    public static void RegisterMappings()
    {
        AccountMappingConfig.Configure();
        RoleMappingConfig.Configure();
        PermissionMappingConfig.Configure();
        MenuMappingConfig.Configure();
    }
}
