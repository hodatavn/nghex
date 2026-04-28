using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nghex.Data.Setup;
using Nghex.Logging.Setup;
using System.Reflection;

namespace Nghex.Logging.Api.Extensions;

public static class LoggingAspNetCoreRegistration
{
    public static IServiceCollection AddLoggingAspNetCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDbTableScript, LoggingTableScript>();
        return services;
    }

    public static IMvcBuilder AddLoggingMvcPart(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.PartManager.ApplicationParts.Add(
            new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(Assembly.GetExecutingAssembly()));
        return mvcBuilder;
    }
}
