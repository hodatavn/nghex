using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nghex.Configuration.Mapping;
using Nghex.Configuration.Setup;
using Nghex.Data.Setup;
using System.Reflection;

namespace Nghex.Configuration.Api.Extensions;

public static class ConfigurationAspNetCoreRegistration
{
    public static IServiceCollection AddConfigurationAspNetCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDbTableScript, ConfigurationTableScript>();
        services.AddConfigurationMappings();
        return services;
    }

    public static IMvcBuilder AddConfigurationMvcPart(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.PartManager.ApplicationParts.Add(
            new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(Assembly.GetExecutingAssembly()));
        return mvcBuilder;
    }
}
