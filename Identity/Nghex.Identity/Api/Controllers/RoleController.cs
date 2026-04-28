using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Identity.Services.Interfaces;
using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Identity.Enum;
using Nghex.Identity.Middleware;

namespace Nghex.Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController(
        IRoleService roleService,
        ILoggingService loggingService,
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IRoleService _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("role-types")]
        public IActionResult GetRoleTypes()
        {
            try
            {
                var isSuperAdmin = User.HasAnyRoleLevel(RoleLevel.SuperAdmin);
                var roleTypes = System.Enum.GetValues<RoleLevel>();
                if (!isSuperAdmin)
                    roleTypes = [.. roleTypes.Where(roleType => roleType != RoleLevel.SuperAdmin)];

                var roleTypeList = roleTypes
                    .Select(roleType => new RoleLevelResponse
                    {
                        Name = roleType.GetDisplayName(),
                        Value = roleType.GetLevel()
                    })
                    .ToList();

                return Ok(new RoleLevelListResponse
                {
                    Levels = roleTypeList,
                    Count = roleTypeList.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("all")]
        public async Task<ActionResult<RoleListResponse>> GetAllRoles()
        {
            StartProcessing();
            try
            {
                bool isSuperAdmin = User.HasAnyRoleLevel(RoleLevel.SuperAdmin);
                var roles = await _roleService.GetAllAsync(!isSuperAdmin);
                var roleList = roles.ToList();

                return await SuccessAsync(new RoleListResponse
                {
                    Roles = roleList,
                    TotalCount = roleList.Count
                }, "Roles retrieved successfully");
            }
            catch (Exception ex) { return await HandleExceptionAsync<RoleListResponse>(ex, "Failed to get roles"); }
            finally { StopProcessing(); }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpPost("create")]
        public async Task<ActionResult<GenericResponseModel>> CreateRole([FromBody] CreateRoleRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<GenericResponseModel>(request.GetValidationErrors());

                request.CreatedBy = User.GetUsername() ?? "system";

                var role = await _roleService.CreateAsync(request);
                return await SuccessAsync(new GenericResponseModel { Data = role }, "Role created successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to create role"); }
            finally { StopProcessing(); }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpPut("update")]
        public async Task<ActionResult<GenericResponseModel>> UpdateRole([FromBody] UpdateRoleRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<GenericResponseModel>(request.GetValidationErrors());

                request.UpdatedBy = User.GetUsername() ?? "system";

                var success = await _roleService.UpdateAsync(request);
                if (!success)
                    return Error<GenericResponseModel>("Failed to update role", "UPDATE_FAILED");

                var role = await _roleService.GetByIdAsync(request.Id);
                if (role == null)
                    return Error<GenericResponseModel>("Role not found after update", "NOT_FOUND");

                return await SuccessAsync(new GenericResponseModel { Data = role }, "Role updated successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to update role"); }
            finally { StopProcessing(); }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<GenericResponseModel>> DeleteRole(long id)
        {
            StartProcessing();
            try
            {
                var success = await _roleService.DeleteAsync(id, User.GetUsername() ?? "system");
                if (!success)
                    return Error<GenericResponseModel>("Failed to delete role", "DELETE_FAILED");

                return await SuccessAsync(new GenericResponseModel { Data = new { Message = "Role deleted successfully", RoleId = id } });
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to delete role"); }
            finally { StopProcessing(); }
        }
    }
}
