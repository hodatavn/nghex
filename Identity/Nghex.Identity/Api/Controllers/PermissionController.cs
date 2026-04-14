using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Identity.Api.Models.Permission;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;
using Nghex.Identity.DTOs.Permissions;
using Nghex.Core.Enum;
using Nghex.Identity.Enum;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Api.Controllers
{
    /// <summary>
    /// Permission Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionController(
        IPermissionService permissionService,
        ILoggingService loggingService,
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IPermissionService _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));

        /// <summary>
        /// Get all permissions
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("all")]
        public async Task<ActionResult<PermissionListResponseModel>> GetAllPermissions()
        {
            StartProcessing();

            try
            {
                var permissionDtos = await _permissionService.GetAllAsync();
                var tree = permissionDtos
                    .GroupBy(p => p.PluginName ?? "Core")
                    .Select(pluginGroup => new PluginNode
                    {
                        PluginName = pluginGroup.Key,
                        Modules = pluginGroup
                            .GroupBy(p => p.Module ?? string.Empty)
                            .Select(moduleGroup => new ModuleNode
                            {
                                Module = moduleGroup.Key,
                                Permissions = moduleGroup.Select(p => p.Adapt<PermissionResponseModel>()).ToList()
                            })
                            .OrderBy(m => m.Module)
                            .ToList()
                    })
                    .OrderBy(p => p.PluginName)
                    .ToList();

                var response = new PermissionListResponseModel
                {
                    PermissionTree = tree,
                    TotalCount = tree.Count
                };

                return await SuccessAsync(response, "Permissions retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<PermissionListResponseModel>(ex, "Failed to get permissions");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Create new permission
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        [HttpPost("create")]
        public async Task<ActionResult<GenericResponseModel>> CreatePermission([FromBody] CreatePermissionRequestModel request)
        {
            StartProcessing();

            try
            {
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors);
                }

                // Map Request -> DTO
                var createDto = request.Adapt<CreatePermissionDto>();
                createDto.CreatedBy = User.GetUsername() ?? "system";

                // Service handles DTO -> Entity mapping internally
                var permissionDto = await _permissionService.CreateAsync(createDto);

                // Map DTO -> Response
                var permissionResponse = permissionDto.Adapt<PermissionResponseModel>();

                var response = new GenericResponseModel
                {
                    Data = permissionResponse
                };

                return await SuccessAsync(response, "Permission created successfully");
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
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to create permission");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Update permission
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        [HttpPut("update")]
        public async Task<ActionResult<GenericResponseModel>> UpdatePermission([FromBody] UpdatePermissionRequestModel request)
        {
            StartProcessing();

            try
            {
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors);
                }

                // Map Request -> DTO
                var updateDto = request.Adapt<UpdatePermissionDto>();
                updateDto.UpdatedBy = User.GetUsername() ?? "system";

                // Service handles DTO -> Entity mapping internally
                var success = await _permissionService.UpdateAsync(updateDto);
                if (!success)
                    return Error<GenericResponseModel>("Failed to update permission", "UPDATE_FAILED");

                // Get updated permission
                var permissionDto = await _permissionService.GetByIdAsync(updateDto.Id);
                var permissionResponse = permissionDto?.Adapt<PermissionResponseModel>();

                var response = new GenericResponseModel
                {
                    Data = permissionResponse
                };

                return await SuccessAsync(response, "Permission updated successfully");
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
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to update permission");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Delete permission
        /// </summary>
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
            catch (InvalidOperationException ex)
            {
                return Error<IdResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<IdResponseModel>(ex, "Failed to delete permission");
            }
            finally
            {
                StopProcessing();
            }
        }
    }
}
