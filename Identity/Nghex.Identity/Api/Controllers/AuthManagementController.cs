using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Identity.Api.Models.AccountRole;
using Nghex.Identity.Api.Models.MenuItemPermission;
using Nghex.Identity.Api.Models.Permission;
using Nghex.Identity.Api.Models.Role;
using Nghex.Identity.Api.Models.RolePermission;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;
using Nghex.Core.Enum;
using Nghex.Identity.Enum;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Api.Controllers
{
    /// <summary>
    /// Auth Management Controller - Manages account-role-permission and menu-permission relationships
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class AuthManagementController(
        IAuthManagementService authManagementService,
        ILoggingService loggingService,
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IAuthManagementService _authManagementService = authManagementService ?? throw new ArgumentNullException(nameof(authManagementService));

        #region Account - Role Management

        /// <summary>
        /// Get all roles of an account
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("roles-of-account/{accountId}")]
        public async Task<ActionResult<RoleListResponseModel>> GetRolesOfAccount(long accountId)
        {
            StartProcessing();

            try
            {
                if (accountId <= 0)
                    return Error<RoleListResponseModel>("Account ID is invalid", "VALIDATION_ERROR");

                var roleDtos = await _authManagementService.GetRolesOfAccountAsync(accountId);
                var roleList = roleDtos.Select(r => r.Adapt<RoleResponseModel>()).ToList();

                return await SuccessAsync(new RoleListResponseModel { Roles = roleList, TotalCount = roleList.Count }, "Roles retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<RoleListResponseModel>(ex, "Failed to get roles for account");
            }
            finally
            {
                StopProcessing();
            }
        }

        

        /// <summary>
        /// Update roles of an account (replaces all existing roles)
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpPost("assign-roles-to-account/{accountId}")]
        public async Task<ActionResult<GenericResponseModel>> AssignRolesToAccount(long accountId, [FromBody] AssignRoleRequestModel request)
        {
            StartProcessing();

            try
            {
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors);
                }
                if (request.AccountId != accountId)
                    return Error<GenericResponseModel>("Account ID not matched with request body", "VALIDATION_ERROR");

                var success = await _authManagementService.AssignRolesToAccountAsync(
                    accountId,
                    request.RoleIds);

                if (!success)
                    return Error<GenericResponseModel>("Failed to update roles for account", "UPDATE_FAILED");

                // Get updated roles
                var roles = await _authManagementService.GetRolesOfAccountAsync(accountId);
                var roleList = roles.Select(r => r.Adapt<RoleResponseModel>()).ToList();

                return await SuccessAsync(new GenericResponseModel { Data = new { AccountId = accountId, Roles = roleList }, TotalCount = roleList.Count }, "Roles assigned successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to assign roles to account");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Remove all roles from an account
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpDelete("remove-all-roles/{accountId}")]
        public async Task<ActionResult<GenericResponseModel>> RemoveAllRolesFromAccount(long accountId)
        {
            StartProcessing();

            try
            {
                if (accountId <= 0)
                    return Error<GenericResponseModel>("Account ID is invalid", "VALIDATION_ERROR");

                var success = await _authManagementService.RemoveAllRolesFromAccountAsync(accountId);

                if (!success)
                    return Error<GenericResponseModel>("Failed to remove all roles from account", "REMOVE_FAILED");

                return await SuccessAsync(new GenericResponseModel { Data = new { AccountId = accountId } }, "All roles removed successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to remove all roles from account");
            }
            finally
            {
                StopProcessing();
            }
        }

        #endregion

        #region Role - Permission Management
        
        /// <summary>
        /// Get all permissions of a role
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("permissions-of-role/{roleId}")]
        public async Task<ActionResult<PermissionListResponseModel>> GetPermissionsOfRole(long roleId)
        {
            StartProcessing();

            try
            {
                if (roleId <= 0)
                    return Error<PermissionListResponseModel>("Role ID is invalid", "VALIDATION_ERROR");

                var permissionDtos = await _authManagementService.GetPermissionsOfRoleAsync(roleId);

                // Build tree structure: PluginName -> Module -> Permission list (group DTOs first, then map)
                var permissionTree = permissionDtos
                    .GroupBy(p => p.PluginName ?? "Core")
                    .Select(pluginGroup => new PluginNode
                    {
                        PluginName = pluginGroup.Key,
                        Modules = pluginGroup
                            .GroupBy(p => p.Module ?? "General")
                            .Select(moduleGroup => new ModuleNode
                            {
                                Module = moduleGroup.Key,
                                Permissions = moduleGroup.Select(p => p.Adapt<PermissionResponseModel>()).ToList()
                            })
                            .ToList()
                    })
                    .ToList();

                var permissionList = permissionDtos.Select(p => p.Adapt<PermissionResponseModel>()).ToList();

                var response = new PermissionListResponseModel
                {
                    PermissionTree = permissionTree,
                    TotalCount = permissionList.Count
                };

                return await SuccessAsync(response, "Permissions retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<PermissionListResponseModel>(ex, "Failed to get permissions for role");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Update permissions on a role (replaces all existing permissions)
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpPost("grant-permissions-to-role/{roleId}")]
        public async Task<ActionResult<GenericResponseModel>> GrantPermissionsToRole(long roleId, [FromBody] GrantPermissionRequestModel request)
        {
            StartProcessing();

            try
            {
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors);
                }

                if (request.RoleId != roleId)
                    return Error<GenericResponseModel>("Role ID not matched with request body", "VALIDATION_ERROR");

                var success = await _authManagementService.GrantPermissionsToRoleAsync(
                    roleId,
                    request.PermissionIds);

                if (!success)
                    return Error<GenericResponseModel>("Failed to update permissions for role", "UPDATE_FAILED");

                // Get updated permissions
                var permissions = await _authManagementService.GetPermissionsOfRoleAsync(roleId);
                var permissionList = permissions.Select(p => p.Adapt<PermissionResponseModel>()).ToList();

                return await SuccessAsync(new GenericResponseModel { Data = new { RoleId = roleId, Permissions = permissionList }, TotalCount = permissionList.Count }, "Permissions granted successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to grant permissions to role");
            }
            finally
            {
                StopProcessing();
            }
        }

        #endregion

        #region Menu - Permission Management

        /// <summary>
        /// Get all permissions of a menu item
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("permissions-on-menu/{menuKey}")]
        public async Task<ActionResult<GenericResponseModel>> GetPermissionsOfMenu(string menuKey)
        {
            StartProcessing();

            try
            {
                if (string.IsNullOrWhiteSpace(menuKey))
                    return Error<GenericResponseModel>("Menu key is invalid", "VALIDATION_ERROR");

                var permissionCodes = await _authManagementService.GetPermissionsOfMenuAsync(menuKey);

                var response = new GenericResponseModel
                {
                    Data = new
                    {
                        MenuKey = menuKey,
                        PermissionCodes = permissionCodes.ToList(),
                    },
                    TotalCount = permissionCodes.Count()
                };

                return await SuccessAsync(response, "Get permissions of menu successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to get permissions for menu");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Get permission candidates for assigning to a menu (filtered by menu's PermissionPrefix)
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("permission-candidates-on-menu/{menuKey}")]
        public async Task<ActionResult<AuthPermissionListResponseModel>> GetPermissionCandidatesOfMenu(string menuKey)
        {
            StartProcessing();

            try
            {
                if (string.IsNullOrWhiteSpace(menuKey))
                    return Error<AuthPermissionListResponseModel>("Menu key is invalid", "VALIDATION_ERROR");

                var permissionDtos = await _authManagementService.GetPermissionCandidatesOfMenuAsync(menuKey);

                // Build tree structure: PluginName -> Module -> Permission list
                var permissionTree = permissionDtos
                    .GroupBy(p => p.PluginName ?? "Core")
                    .Select(pluginGroup => new AuthPluginNode
                    {
                        PluginName = pluginGroup.Key,
                        Modules = pluginGroup
                            .GroupBy(p => p.Module ?? "General")
                            .Select(moduleGroup => new AuthModuleNode
                            {
                                Module = moduleGroup.Key,
                                Permissions = moduleGroup.Select(p => p.Adapt<PermissionWithAssignStatusResponseModel>()).ToList()
                            })
                            .ToList()
                    })
                    .ToList();

                var response = new AuthPermissionListResponseModel
                {
                    PermissionTree = permissionTree,
                    TotalCount = permissionDtos.Count()
                };

                return await SuccessAsync(response, "Permission candidates retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<AuthPermissionListResponseModel>(ex, "Failed to get permission candidates for menu");
            }
            finally
            {
                StopProcessing();
            }
        }


        /// <summary>
        /// Update permissions on a menu item (replaces all existing permissions)
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpPost("set-permissions-on-menu/{menuKey}")]
        public async Task<ActionResult<GenericResponseModel>> SetPermissionsOnMenu(string menuKey, [FromBody] SetPermissionsOnMenuRequestModel request)
        {
            StartProcessing();

            try
            {
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors);
                }

                if (request.MenuKey != menuKey)
                    return Error<GenericResponseModel>("Menu key not matched with request body", "VALIDATION_ERROR");

                var success = await _authManagementService.SetPermissionsOnMenuAsync(
                    menuKey,
                    request.PermissionCodes);

                if (!success)
                    return Error<GenericResponseModel>("Failed to set permissions on menu", "SET_FAILED");

                // Get updated permissions
                var permissionCodes = await _authManagementService.GetPermissionsOfMenuAsync(menuKey);

                var response = new GenericResponseModel
                {
                    Data = new
                    {
                        MenuKey = menuKey,
                        PermissionCodes = permissionCodes.ToList()
                    },
                    TotalCount = permissionCodes.Count()
                };

                return await SuccessAsync(response, "Set permissions on menu successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to set permissions on menu");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Remove specific permissions from a menu item
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpDelete("remove-permissions-from-menu/{menuKey}")]
        public async Task<ActionResult<GenericResponseModel>> RemovePermissionsFromMenu(string menuKey, [FromBody] RemoveMenuPermissionsRequestModel request)
        {
            StartProcessing();

            try
            {
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors);
                }

                if (request.MenuKey != menuKey)
                    return Error<GenericResponseModel>("Menu key not matched with request body", "VALIDATION_ERROR");

                var success = await _authManagementService.RemovePermissionsFromMenuAsync(
                    menuKey,
                    request.PermissionCodes);

                if (!success)
                    return Error<GenericResponseModel>("Failed to remove permissions from menu", "REMOVE_FAILED");

                var response = new GenericResponseModel
                {
                    Data = new
                    {
                        MenuKey = menuKey,
                        PermissionCodes = request.PermissionCodes
                    },
                    TotalCount = request.PermissionCodes.Count
                };

                return await SuccessAsync(response, "Remove permissions from menu successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to remove permissions from menu");
            }
            finally
            {
                StopProcessing();
            }
        }

        #endregion
    }
}