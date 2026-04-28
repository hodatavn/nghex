using Mapster;
using Nghex.Configuration.Api.Models;
using Nghex.Configuration.Persistence.Entities;

namespace Nghex.Configuration.Mapping;

public static class ConfigurationMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<ConfigurationEntity, ConfigurationResponseModel>.NewConfig()
            .Map(dest => dest.ConfigurationId, src => src.Id);

        TypeAdapterConfig<CreateConfigurationRequest, ConfigurationEntity>.NewConfig()
            .Map(dest => dest.Key, src => src.Key)
            .Map(dest => dest.Value, src => src.Value)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.DataType, src => src.DataType)
            .Map(dest => dest.Module, src => src.Module)
            .Map(dest => dest.IsSystemConfig, src => src.IsSystemConfig)
            .Map(dest => dest.IsEditable, src => src.IsEditable)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.DefaultValue, src => src.DefaultValue)
            .Map(dest => dest.CreatedBy, src => src.CreatedBy)
            .Ignore(dest => dest.Id!)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.UpdatedBy!);

        TypeAdapterConfig<UpdateConfigurationRequest, ConfigurationEntity>.NewConfig()
            .Map(dest => dest.Value, src => src.Value)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.DataType, src => src.DataType)
            .Map(dest => dest.Module, src => src.Module)
            .Map(dest => dest.IsEditable, src => src.IsEditable)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.DefaultValue, src => src.DefaultValue)
            .Map(dest => dest.UpdatedBy, src => src.UpdatedBy)
            .Ignore(dest => dest.Key!)
            .Ignore(dest => dest.Id!)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.CreatedBy!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.IsSystemConfig!);
    }
}
