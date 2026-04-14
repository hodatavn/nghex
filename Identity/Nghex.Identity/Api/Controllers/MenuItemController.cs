using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Identity.Api.Models.MenuItem;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;
using Nghex.Identity.DTOs.Menus;
using Nghex.Core.Enum;
using Nghex.Identity.Enum;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Api.Controllers
{
    /// <summary>
    /// Menu Item Controller - CRUD operations for menu items and permission management
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class MenuItemController(
        IMenuService menuService,
        ILoggingService loggingService,
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IMenuService _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));

        #region Basic CRUD Operations

        /// <summary>
        /// Get all menu items
        /// </summary>
        [HttpGet("all")]
        [AuthorizeByRoleLevel(RoleLevel.Admin, RoleLevel.SuperAdmin)]
        public async Task<ActionResult<MenuItemListResponseModel>> GetAllMenuItems([FromQuery] bool activeOnly = false)
        {
            StartProcessing();

            try
            {
                var menuDtos = await _menuService.GetAllAsync(activeOnly);
                var menuList = menuDtos.Select(m => m.Adapt<MenuItemResponseModel>()).ToList();

                var response = new MenuItemListResponseModel
                {
                    MenuItems = menuList,
                    TotalCount = menuList.Count
                };

                return await SuccessAsync(response, "Menu items retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<MenuItemListResponseModel>(ex, "Failed to get menu items");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Get menu item by key
        /// </summary>
        [HttpGet("key/{menuKey}")]
        [AuthorizeByRoleLevel(RoleLevel.Admin, RoleLevel.SuperAdmin)]
        public async Task<ActionResult<GenericResponseModel>> GetMenuItemByKey(string menuKey)
        {
            StartProcessing();

            try
            {
                var menuDto = await _menuService.GetByMenuKeyAsync(menuKey);
                if (menuDto == null)
                    return NotFound<GenericResponseModel>("Menu not found");

                var response = new GenericResponseModel
                {
                    Data = menuDto.Adapt<MenuItemResponseModel>()
                };

                return await SuccessAsync(response, "Menu retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to get menu");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Create new menu item
        /// </summary>
        [HttpPost("create")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        public async Task<ActionResult<GenericResponseModel>> CreateMenuItem([FromBody] CreateMenuItemRequestModel request)
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
                var createDto = request.Adapt<CreateMenuItemDto>();
                createDto.CreatedBy = User.GetUsername() ?? "system";

                // Service handles DTO -> Entity mapping internally
                var menuDto = await _menuService.CreateAsync(createDto);

                // Map DTO -> Response
                var menuResponse = menuDto.Adapt<MenuItemResponseModel>();

                var response = new GenericResponseModel
                {
                    Data = menuResponse
                };

                return await SuccessAsync(response, "Menu created successfully");
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
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to create menu");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Update menu item
        /// </summary>
        [HttpPut("update")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        public async Task<ActionResult<GenericResponseModel>> UpdateMenuItem([FromBody] UpdateMenuItemRequestModel request)
        {
            StartProcessing();

            try
            {
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors);
                }

                // Get existing menu to get the ID
                var existingMenu = await _menuService.GetByMenuKeyAsync(request.MenuKey);
                if (existingMenu == null)
                    return NotFound<GenericResponseModel>("Menu not found");

                // Map Request -> DTO
                var updateDto = request.Adapt<UpdateMenuItemDto>();
                updateDto.Id = existingMenu.Id;  // Use existing menu's ID
                updateDto.UpdatedBy = User.GetUsername() ?? "system";

                // Service handles DTO -> Entity mapping internally
                var success = await _menuService.UpdateAsync(updateDto);
                if (!success)
                    return Error<GenericResponseModel>("Failed to update menu", "UPDATE_FAILED");

                // Get updated menu
                var menuDto = await _menuService.GetByMenuKeyAsync(request.MenuKey);
                var menuResponse = menuDto?.Adapt<MenuItemResponseModel>();

                var response = new GenericResponseModel
                {
                    Data = menuResponse
                };

                return await SuccessAsync(response, "Menu updated successfully");
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
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to update menu");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Delete menu item
        /// </summary>
        [HttpDelete("delete/{id}")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        public async Task<ActionResult<GenericResponseModel>> DeleteMenuItem(long id)
        {
            StartProcessing();

            try
            {
                var success = await _menuService.DeleteAsync(id, User.GetUsername() ?? "system");
                if (!success)
                    return Error<GenericResponseModel>("Failed to delete menu", "DELETE_FAILED");

                return await SuccessAsync(new GenericResponseModel { Data = new { Message = "Menu deleted successfully" } });
            }
            catch (InvalidOperationException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to delete menu");
            }
            finally
            {
                StopProcessing();
            }
        }

        #endregion
    }
}
