using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Nghex.Identity.Api.Extensions;

public static class IdentityAspNetCoreRegistration
{
    public static IServiceCollection AddIdentityAspNetCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        return services;
    }

    public static IMvcBuilder AddIdentityMvcPart(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.PartManager.ApplicationParts.Add(
            new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(Assembly.GetExecutingAssembly()));
        return mvcBuilder;
    }
}
