using Mapster;
using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Mapping;

public static class AccountMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<AccountEntity, AccountResponse>.NewConfig()
            .Map(dest => dest.AccountId, src => src.Id);

        TypeAdapterConfig<CreateAccountRequest, AccountEntity>.NewConfig()
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

        TypeAdapterConfig<UpdateAccountRequest, AccountEntity>.NewConfig()
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
