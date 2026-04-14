using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Identity.Services.Interfaces;
using Nghex.Identity.Api.Models.Role;
using Nghex.Core.Enum;
using Nghex.Identity.Enum;
using Nghex.Identity.Api.Models;
using Nghex.Identity.Middleware;
using Nghex.Identity.DTOs.Roles;

namespace Nghex.Identity.Api.Controllers
{
    /// <summary>
    /// Role Controller - CRUD operations for roles
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController(
        IRoleService roleService,
        ILoggingService loggingService,
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IRoleService _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

        /// <summary>
        /// Get all role levels/types
        /// </summary>
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
                    .Select(roleType => new RoleLevelResponseModel
                    {
                        Name = roleType.GetDisplayName(),
                        Value = roleType.GetLevel()
                    })
                    .ToList();
                    
                return Ok(new RoleLevelListResponseModel
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
        
        /// <summary>
        /// Get all roles
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("all")]
        public async Task<ActionResult<RoleListResponseModel>> GetAllRoles()
        {
            StartProcessing();

            try
            {
                bool isSuperAdmin = User.HasAnyRoleLevel(RoleLevel.SuperAdmin);
                var roleDtos = await _roleService.GetAllAsync(!isSuperAdmin);

                // Map DTOs to Response Models using Mapster
                var roleList = roleDtos
                    .Select(dto => new RoleResponseModel
                    {
                        RoleId = dto.Id,
                        RoleCode = dto.Code,
                        RoleName = dto.Name,
                        Description = dto.Description,
                        RoleLevel = dto.RoleLevel,
                        IsActive = dto.IsActive
                    }).ToList();

                var response = new RoleListResponseModel
                {
                    Roles = roleList,
                    TotalCount = roleList.Count
                };

                return await SuccessAsync(response, "Roles retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<RoleListResponseModel>(ex, "Failed to get roles");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Create new role
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpPost("create")]
        public async Task<ActionResult<GenericResponseModel>> CreateRole([FromBody] CreateRoleRequestModel request)
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
                var createDto = request.Adapt<CreateRoleDto>();
                createDto.CreatedBy = User.GetUsername() ?? "system";
                
                // Service handles DTO -> Entity mapping internally
                var roleDto = await _roleService.CreateAsync(createDto);

                // Map DTO -> Response
                var roleResponse = new RoleResponseModel
                {
                    RoleId = roleDto.Id,
                    RoleCode = roleDto.Code,
                    RoleName = roleDto.Name,
                    Description = roleDto.Description,
                    RoleLevel = roleDto.RoleLevel,
                    IsActive = roleDto.IsActive
                };

                var response = new GenericResponseModel
                {
                    Data = roleResponse
                };

                return await SuccessAsync(response, "Role created successfully");
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
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to create role");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Update role
        /// </summary>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpPut("update")]
        public async Task<ActionResult<GenericResponseModel>> UpdateRole([FromBody] UpdateRoleRequestModel request)
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
                var updateDto = request.Adapt<UpdateRoleDto>();
                updateDto.UpdatedBy = User.GetUsername() ?? "system";
                
                // Service handles DTO -> Entity mapping internally
                var success = await _roleService.UpdateAsync(updateDto);
                
                if (!success)
                    return Error<GenericResponseModel>("Failed to update role", "UPDATE_FAILED");

                // Get updated role
                var roleDto = await _roleService.GetByIdAsync(updateDto.Id);
                if (roleDto == null)
                    return Error<GenericResponseModel>("Role not found after update", "NOT_FOUND");

                // Map DTO -> Response
                var roleResponse = new RoleResponseModel
                {
                    RoleId = roleDto.Id,
                    RoleCode = roleDto.Code,
                    RoleName = roleDto.Name,
                    Description = roleDto.Description,
                    RoleLevel = roleDto.RoleLevel,
                    IsActive = roleDto.IsActive
                };

                var response = new GenericResponseModel
                {
                    Data = roleResponse
                };

                return await SuccessAsync(response, "Role updated successfully");
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
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to update role");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Delete role
        /// </summary>
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
                    
                return await SuccessAsync(new GenericResponseModel 
                { 
                    Data = new { Message = "Role deleted successfully", RoleId = id } 
                });
            }
            catch (InvalidOperationException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to delete role");
            }
            finally
            {
                StopProcessing();
            }
        }

        // #region Permission Management

        // /// <summary>
        // /// Update permissions on a role (batch operation)
        // /// </summary>
        // [HttpPost("assign-permissions")]
        // [AuthorizeByRoleLevel(RoleLevel.Admin, RoleLevel.SuperAdmin)]
        // public async Task<ActionResult<GenericResponseModel>> UpdatePermissionsOnRole([FromBody] GrantPermissionRequestModel request)
        // {
        //     StartProcessing();

        //     try
        //     {
        //         if (!request.IsValid())
        //         {
        //             var errors = request.GetValidationErrors();
        //             return ValidationError<GenericResponseModel>(errors);
        //         }

        //         var success = await _roleService.UpdatePermissionsOnRoleAsync(
        //             request.RoleId, 
        //             request.PermissionIds, 
        //             User.GetUsername() ?? "system");
                    
        //         if (!success)
        //             return Error<GenericResponseModel>("Failed to assign permissions", "ASSIGN_FAILED");

        //         var permissionDtos = await _roleService.GetPermissionsOfRoleAsync(request.RoleId);
                
        //         // Map DTOs to Response Models using Mapster
        //         var permissionList = permissionDtos.Select(p => new PermissionResponseModel 
        //         { 
        //             Id = p.Id, 
        //             Code = p.Code, 
        //             Name = p.Name, 
        //             Description = p.Description, 
        //             IsActive = p.IsActive 
        //         }).ToList();

        //         var response = new GenericResponseModel
        //         {
        //             Data = new
        //             {
        //                 RoleId = request.RoleId,
        //                 Permissions = permissionList
        //             }
        //         };

        //         return await SuccessAsync(response, "Permissions assigned successfully");
        //     }
        //     catch (Exception ex)
        //     {
        //         return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to assign permissions");
        //     }
        //     finally
        //     {
        //         StopProcessing();
        //     }
        // }

        // #endregion
    }
}
