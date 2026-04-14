using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Identity.Api.Models.Account;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;
using Nghex.Identity.DTOs.Accounts;
using Nghex.Core.Enum;
using Nghex.Identity.Enum;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Api.Controllers
{
    /// <summary>
    /// Account Controller
    /// </summary>
    [Route("api/[controller]")]
    public class AccountController(
        IAccountService accountService, 
        ILoggingService loggingService, 
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IAccountService _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

        /// <summary>
        /// Get all accounts
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("all")]
        public async Task<ActionResult<AccountListResponseModel>> GetAllAccounts(bool isDeleted = false)
        {
            StartProcessing();

            try
            {
                var accountDtos = await _accountService.GetAllAsync(isDeleted);
                var accountList = accountDtos.Select(a => a.Adapt<AccountResponseModel>()).ToList();

                var response = new AccountListResponseModel
                {
                    Accounts = accountList,
                    TotalCount = accountList.Count
                };

                return await SuccessAsync(response, "Accounts retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<AccountListResponseModel>(ex, "Failed to get all accounts");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Register new account
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<GenericResponseModel>> Register([FromBody] CreateAccountRequestModel request)
        {
            StartProcessing();

            try
            {
                // Validate request
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors).Result!;
                }

                // Map Request -> DTO
                var createDto = request.Adapt<CreateAccountDto>();
                createDto.CreatedBy = User.GetUsername() ?? "system";

                // Service handles DTO -> Entity mapping internally
                var accountDto = await _accountService.CreateAsync(createDto);

                // Map DTO -> Response
                var accountResponse = accountDto.Adapt<AccountResponseModel>();

                var response = new GenericResponseModel
                {
                    Data = accountResponse
                };

                return await SuccessAsync(response, "Account registered successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (ArgumentException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to register account");
            }
            finally
            {
                StopProcessing();
            }
        }


        /// <summary>
        /// Get account information
        /// </summary>
        [HttpGet("info/{username}")]
        [Authorize]
        public async Task<ActionResult<GenericResponseModel>> GetAccountInfo(string username)
        {
            StartProcessing();

            try
            {
                var accountDto = await _accountService.GetByUsernameAsync(username);

                if (accountDto == null)
                    return NotFound<GenericResponseModel>("Account not found").Result!;

                var response = new GenericResponseModel
                {
                    Data = accountDto.Adapt<AccountResponseModel>()
                };

                return await SuccessAsync(response, "Account retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to get account");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Update account information
        /// </summary>
        [HttpPut("update")]
        [Authorize]
        public async Task<ActionResult<GenericResponseModel>> UpdateAccount([FromBody] UpdateAccountRequestModel request)
        {
            StartProcessing();

            try
            {
                // Validate request
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors).Result!;
                }
                
                // Map Request -> DTO
                var updateDto = request.Adapt<UpdateAccountDto>();
                updateDto.UpdatedBy = User.GetUsername() ?? "system";

                // Service handles DTO -> Entity mapping internally
                if (!await _accountService.UpdateAsync(updateDto))
                    return Error<GenericResponseModel>("Failed to update account", "UPDATE_FAILED", "Update operation failed").Result!;

                // Get updated account
                var accountDto = await _accountService.GetByUsernameAsync(updateDto.Username);
                var accountResponse = accountDto?.Adapt<AccountResponseModel>();

                return await SuccessAsync(new GenericResponseModel { Data = accountResponse }, "Account updated successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (ArgumentException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to update account");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Change account password
        /// </summary>
        [HttpPut("change-password")]
        [Authorize]
        public async Task<ActionResult<GenericResponseModel>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            StartProcessing();
            try
            {
                // Validate request
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors).Result!;
                }

                var success = await _accountService.ChangePasswordAsync(request.Username, request.CurrentPassword, request.NewPassword);
                if (!success)
                    return Error<GenericResponseModel>("Failed to change password", "CHANGE_PASSWORD_FAILED", "Change password operation failed").Result!;

                return await SuccessAsync(new GenericResponseModel { Data = new { Message = "Password changed successfully" } });
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to change password");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Delete account
        /// </summary>
        [HttpDelete("delete/{id}")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        public async Task<ActionResult<GenericResponseModel>> DeleteAccount(long id)
        {
            StartProcessing();

            try
            {
                var success = await _accountService.DeleteAsync(id, User.GetUsername() ?? "system");
                if (!success)
                    return Error<GenericResponseModel>("Failed to delete account", "DELETE_FAILED", "Delete operation failed").Result!;

                var response = new GenericResponseModel
                {
                    Data = new { Message = "Account deleted successfully", AccountId = id }
                };

                return await SuccessAsync(response, "Account deleted successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to delete account");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Restore account
        /// </summary>
        [HttpPost("restore/{id}")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        public async Task<ActionResult<GenericResponseModel>> RestoreAccount(long id)
        {
            StartProcessing();

            try
            {
                var success = await _accountService.RestoreAsync(id, User.GetUsername() ?? "system");
                if (!success)
                    return Error<GenericResponseModel>("Failed to restore account", "RESTORE_FAILED", "Restore operation failed").Result!;

                var response = new GenericResponseModel
                {
                    Data = new { Message = "Account restored successfully", AccountId = id }
                };

                return await SuccessAsync(response, "Account restored successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to restore account");
            }
            finally
            {
                StopProcessing();
            }

        }

        /// <summary>
        /// Reset password
        /// </summary>
        [HttpPost("reset-password/{username}")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        public async Task<ActionResult<GenericResponseModel>> ResetPassword(string username)
        {
            StartProcessing();
            
            try
            {
                var success = await _accountService.ResetPasswordAsync(username);
                if (!success)
                    return Error<GenericResponseModel>("Failed to reset password", "RESET_PASSWORD_FAILED", "Reset password operation failed").Result!;

                var response = new GenericResponseModel
                {
                    Data = new { Message = "Password reset successfully" }
                };

                return await SuccessAsync(response, "Password reset successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to reset password");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Search accounts
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<AccountListResponseModel>> SearchAccounts([FromBody] SearchAccountRequestModel request)
        {
            StartProcessing();

            try
            {
                // Validate request
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<AccountListResponseModel>(errors).Result!;
                }

                // TODO: Implement search logic với IAccountService
                // For now, return empty result
                var response = new AccountListResponseModel
                {
                    Accounts = new List<AccountResponseModel>(),
                    TotalCount = 0,
                };

                return await SuccessAsync(response, "Search completed");
            }
            catch (InvalidOperationException ex)
            {
                return Error<AccountListResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (ArgumentException ex)
            {
                return Error<AccountListResponseModel>(ex.Message, "VALIDATION_ERROR");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<AccountListResponseModel>(ex, "Failed to search accounts");
            }
            finally
            {
                StopProcessing();
            }
        }
    }
}
