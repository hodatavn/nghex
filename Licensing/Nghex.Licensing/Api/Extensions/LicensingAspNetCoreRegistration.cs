using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Nghex.Licensing.Api.Extensions;

public static class LicensingAspNetCoreRegistration
{
    public static IServiceCollection AddLicensingAspNetCore(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    public static IMvcBuilder AddLicensingMvcPart(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.PartManager.ApplicationParts.Add(
            new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(Assembly.GetExecutingAssembly()));
        return mvcBuilder;
    }
}
