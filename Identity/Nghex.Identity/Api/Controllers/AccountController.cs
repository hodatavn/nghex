using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;
using Nghex.Identity.Enum;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Api.Controllers
{
    [Route("api/[controller]")]
    public class AccountController(
        IAccountService accountService,
        ILoggingService loggingService,
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IAccountService _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("all")]
        public async Task<ActionResult<AccountListResponse>> GetAllAccounts(bool isDeleted = false)
        {
            StartProcessing();
            try
            {
                var accounts = await _accountService.GetAllAsync(isDeleted);
                var response = new AccountListResponse
                {
                    Accounts = accounts.ToList(),
                    TotalCount = accounts.Count()
                };
                return await SuccessAsync(response, "Accounts retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<AccountListResponse>(ex, "Failed to get all accounts");
            }
            finally { StopProcessing(); }
        }

        [HttpPost("register")]
        public async Task<ActionResult<GenericResponseModel>> Register([FromBody] CreateAccountRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<GenericResponseModel>(request.GetValidationErrors()).Result!;

                request.CreatedBy = User.GetUsername() ?? "system";

                var account = await _accountService.CreateAsync(request);
                return await SuccessAsync(new GenericResponseModel { Data = account }, "Account registered successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to register account"); }
            finally { StopProcessing(); }
        }

        [HttpGet("info/{username}")]
        [Authorize]
        public async Task<ActionResult<GenericResponseModel>> GetAccountInfo(string username)
        {
            StartProcessing();
            try
            {
                var account = await _accountService.GetByUsernameAsync(username);
                if (account == null)
                    return NotFound<GenericResponseModel>("Account not found").Result!;

                return await SuccessAsync(new GenericResponseModel { Data = account }, "Account retrieved successfully");
            }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to get account"); }
            finally { StopProcessing(); }
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<ActionResult<GenericResponseModel>> UpdateAccount([FromBody] UpdateAccountRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<GenericResponseModel>(request.GetValidationErrors()).Result!;

                request.UpdatedBy = User.GetUsername() ?? "system";

                if (!await _accountService.UpdateAsync(request))
                    return Error<GenericResponseModel>("Failed to update account", "UPDATE_FAILED", "Update operation failed").Result!;

                var account = await _accountService.GetByUsernameAsync(request.Username);
                return await SuccessAsync(new GenericResponseModel { Data = account }, "Account updated successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to update account"); }
            finally { StopProcessing(); }
        }

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

                return await SuccessAsync(new GenericResponseModel { Data = new { Message = "Account deleted successfully", AccountId = id } }, "Account deleted successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to delete account"); }
            finally { StopProcessing(); }
        }

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

                return await SuccessAsync(new GenericResponseModel { Data = new { Message = "Account restored successfully", AccountId = id } }, "Account restored successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to restore account"); }
            finally { StopProcessing(); }
        }

        [HttpPost("reset-password")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        public async Task<ActionResult<GenericResponseModel>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            StartProcessing();
            try
            {
                var success = await _accountService.ResetPasswordAsync(request.Username);
                if (!success)
                    return Error<GenericResponseModel>("Failed to reset password", "RESET_PASSWORD_FAILED", "Reset password operation failed").Result!;

                return await SuccessAsync(new GenericResponseModel { Data = new { Message = "Password reset successfully" } }, "Password reset successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to reset password"); }
            finally { StopProcessing(); }
        }

        [HttpPost("search")]
        public async Task<ActionResult<AccountListResponse>> SearchAccounts([FromBody] SearchAccountRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<AccountListResponse>(request.GetValidationErrors()).Result!;

                var response = new AccountListResponse
                {
                    Accounts = new List<AccountResponse>(),
                    TotalCount = 0,
                };
                return await SuccessAsync(response, "Search completed");
            }
            catch (InvalidOperationException ex) { return Error<AccountListResponse>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<AccountListResponse>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<AccountListResponse>(ex, "Failed to search accounts"); }
            finally { StopProcessing(); }
        }
    }
}
