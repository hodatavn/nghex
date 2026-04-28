using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Identity.Api.Models.Requests.DataPolicy;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Identity.Enum;
using Nghex.Identity.Middleware;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Identity.Services.Interfaces;
using Nghex.Logging.Interfaces;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;

namespace Nghex.Identity.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class AccessPolicyController(
    IAccessPolicyService accessPolicyService,
    ILoggingService loggingService,
    IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
{
    [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
    [HttpGet("{accountId}")]
    public async Task<ActionResult<AccessPolicyResponse>> GetByAccountId(long accountId)
    {
        StartProcessing();
        try
        {
            var result = await accessPolicyService.GetByAccountIdAsync(accountId);
            return await SuccessAsync(result);
        }
        catch (Exception ex) { return await HandleExceptionAsync<AccessPolicyResponse>(ex, "Failed to get access policy"); }
        finally { StopProcessing(); }
    }

    [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
    [HttpPut("upsert")]
    public async Task<ActionResult<GenericResponseModel>> Upsert([FromBody] SavePolicyModeRequest request)
    {
        StartProcessing();
        try
        {
            if (!request.IsValid(out var error))
                return ValidationError<GenericResponseModel>([error]);

            request.UpdatedBy = User.GetUsername() ?? "system";
            await accessPolicyService.UpsertAsync(request.AccountId, request.PolicyType, request.Mode, request.UpdatedBy);
            return await SuccessAsync(new GenericResponseModel
            {
                Data = new { request.AccountId, request.PolicyType, request.Mode }
            });
        }
        catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to upsert policy"); }
        finally { StopProcessing(); }
    }

    [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
    [HttpPut("details")]
    public async Task<ActionResult<GenericResponseModel>> SaveDetails([FromBody] SavePolicyDetailsRequest request)
    {
        StartProcessing();
        try
        {
            if (!request.IsValid(out var error))
                return ValidationError<GenericResponseModel>([error]);

            request.UpdatedBy = User.GetUsername() ?? "system";
            await accessPolicyService.SaveDetailsAsync(request.AccountId, request.PolicyType, request.PoCodes, request.UpdatedBy);
            return await SuccessAsync(new GenericResponseModel
            {
                Data = new { request.AccountId, request.PolicyType, Count = request.PoCodes.Count }
            });
        }
        catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to save policy details"); }
        finally { StopProcessing(); }
    }

    [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
    [HttpDelete("{accountId}/{policyType}")]
    public async Task<ActionResult<GenericResponseModel>> DeleteByType(long accountId, string policyType)
    {
        StartProcessing();
        try
        {
            await accessPolicyService.DeleteByTypeAsync(accountId, policyType, User.GetUsername() ?? "system");
            return await SuccessAsync(new GenericResponseModel
            {
                Data = new { accountId, policyType, Message = "Policy deleted" }
            });
        }
        catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to delete policy"); }
        finally { StopProcessing(); }
    }

    [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
    [HttpDelete("{accountId}")]
    public async Task<ActionResult<GenericResponseModel>> DeleteAll(long accountId)
    {
        StartProcessing();
        try
        {
            await accessPolicyService.DeleteAllAsync(accountId, User.GetUsername() ?? "system");
            return await SuccessAsync(new GenericResponseModel
            {
                Data = new { accountId, Message = "All policies deleted" }
            });
        }
        catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to delete all policies"); }
        finally { StopProcessing(); }
    }
}
