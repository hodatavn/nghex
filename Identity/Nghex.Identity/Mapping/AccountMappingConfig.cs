using Mapster;
using Nghex.Identity.DTOs.Accounts;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Mapping;

/// <summary>
/// Mapster mapping configuration for Account domain
/// </summary>
public static class AccountMappingConfig
{
    /// <summary>
    /// Configure Account mappings
    /// </summary>
    public static void Configure()
    {
        // Entity -> DTO
        TypeAdapterConfig<AccountEntity, AccountDto>.NewConfig();

        // CreateDto -> Entity
        TypeAdapterConfig<CreateAccountDto, AccountEntity>.NewConfig()
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Password, src => src.Password)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.DisplayName, src => src.DisplayName)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.CreatedBy, src => src.CreatedBy)
            .Ignore(dest => dest.Id!)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.UpdatedBy!)
            .Ignore(dest => dest.IsDeleted!)
            .Ignore(dest => dest.IsLocked!)
            .Ignore(dest => dest.LastLoginAt!)
            .Ignore(dest => dest.FailedLoginAttempts!)
            .Ignore(dest => dest.LockedUntil!)
            .Ignore(dest => dest.IpAddress!);

        // UpdateDto -> Entity
        TypeAdapterConfig<UpdateAccountDto, AccountEntity>.NewConfig()
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.DisplayName, src => src.DisplayName)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.UpdatedBy, src => src.UpdatedBy)
            .Ignore(dest => dest.Id!)
            .Ignore(dest => dest.Password!)
            .Ignore(dest => dest.CreatedAt!)
            .Ignore(dest => dest.CreatedBy!)
            .Ignore(dest => dest.UpdatedAt!)
            .Ignore(dest => dest.IsDeleted!)
            .Ignore(dest => dest.IsLocked!)
            .Ignore(dest => dest.LastLoginAt!)
            .Ignore(dest => dest.FailedLoginAttempts!)
            .Ignore(dest => dest.LockedUntil!)
            .Ignore(dest => dest.IpAddress!);
    }
}
