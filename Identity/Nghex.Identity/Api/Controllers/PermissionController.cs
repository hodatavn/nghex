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
    [Authorize]
    public class PermissionController(
        IPermissionService permissionService,
        ILoggingService loggingService,
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IPermissionService _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("all")]
        public async Task<ActionResult<PermissionListResponse>> GetAllPermissions()
        {
            StartProcessing();
            try
            {
                var permissions = await _permissionService.GetAllAsync();
                var tree = permissions
                    .GroupBy(p => p.PluginName ?? "Core")
                    .Select(pluginGroup => new PluginNode
                    {
                        PluginName = pluginGroup.Key,
                        Modules = pluginGroup
                            .GroupBy(p => p.Module ?? string.Empty)
                            .Select(moduleGroup => new ModuleNode
                            {
                                Module = moduleGroup.Key,
                                Permissions = moduleGroup.ToList()
                            })
                            .OrderBy(m => m.Module)
                            .ToList()
                    })
                    .OrderBy(p => p.PluginName)
                    .ToList();

                return await SuccessAsync(new PermissionListResponse
                {
                    PermissionTree = tree,
                    TotalCount = tree.Count
                }, "Permissions retrieved successfully");
            }
            catch (Exception ex) { return await HandleExceptionAsync<PermissionListResponse>(ex, "Failed to get permissions"); }
            finally { StopProcessing(); }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        [HttpPost("create")]
        public async Task<ActionResult<GenericResponseModel>> CreatePermission([FromBody] CreatePermissionRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<GenericResponseModel>(request.GetValidationErrors());

                request.CreatedBy = User.GetUsername() ?? "system";

                var permission = await _permissionService.CreateAsync(request);
                return await SuccessAsync(new GenericResponseModel { Data = permission }, "Permission created successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to create permission"); }
            finally { StopProcessing(); }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        [HttpPut("update")]
        public async Task<ActionResult<GenericResponseModel>> UpdatePermission([FromBody] UpdatePermissionRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<GenericResponseModel>(request.GetValidationErrors());

                request.UpdatedBy = User.GetUsername() ?? "system";

                var success = await _permissionService.UpdateAsync(request);
                if (!success)
                    return Error<GenericResponseModel>("Failed to update permission", "UPDATE_FAILED");

                var permission = await _permissionService.GetByIdAsync(request.Id);
                return await SuccessAsync(new GenericResponseModel { Data = permission }, "Permission updated successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to update permission"); }
            finally { StopProcessing(); }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<IdResponseModel>> DeletePermission(long id)
        {
            StartProcessing();
            try
            {
                var success = await _permissionService.DeleteAsync(id, User.GetUsername() ?? "system");
                if (!success)
                    return Error<IdResponseModel>("Failed to delete permission", "DELETE_FAILED");

                return await SuccessAsync(new IdResponseModel { Id = id, Data = new { Message = "Permission deleted successfully" } });
            }
            catch (InvalidOperationException ex) { return Error<IdResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (Exception ex) { return await HandleExceptionAsync<IdResponseModel>(ex, "Failed to delete permission"); }
            finally { StopProcessing(); }
        }
    }
}
