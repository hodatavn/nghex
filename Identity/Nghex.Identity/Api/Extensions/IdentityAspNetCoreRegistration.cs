using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nghex.Data.Setup;
using Nghex.Identity.AccessPolicy;
using Nghex.Identity.Mapping;
using Nghex.Identity.Repositories.DataPolicy;
using Nghex.Identity.Repositories.DataPolicy.Interfaces;
using Nghex.Identity.Services.Accounts;
using Nghex.Identity.Services.Interfaces;
using Nghex.Identity.Setup;
using Nghex.Plugins.Abstractions.DataPolicy;
using System.Reflection;

namespace Nghex.Identity.Api.Extensions;

public static class IdentityAspNetCoreRegistration
{
    public static IServiceCollection AddIdentityAspNetCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDbTableScript, IdentityTableScript>();
        services.AddSingleton<IDbTableScript, AccessPolicyTableScript>();

        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddIdentityMappings();

        services.AddSingleton<IAccessPolicyCache, AccessPolicyCache>();
        services.AddScoped<IAccessPolicyContext, AccessPolicyContext>();
        services.AddScoped<IAccessPolicyRepository, AccessPolicyRepository>();
        services.AddScoped<IAccessPolicyService, AccessPolicyService>();

        return services;
    }

    public static IMvcBuilder AddIdentityMvcPart(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.PartManager.ApplicationParts.Add(
            new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(Assembly.GetExecutingAssembly()));
        return mvcBuilder;
    }
}
