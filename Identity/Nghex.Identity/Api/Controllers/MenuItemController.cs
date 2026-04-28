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
    public class MenuItemController(
        IMenuService menuService,
        ILoggingService loggingService,
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IMenuService _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));

        [HttpGet("all")]
        [AuthorizeByRoleLevel(RoleLevel.Admin, RoleLevel.SuperAdmin)]
        public async Task<ActionResult<MenuItemListResponse>> GetAllMenuItems([FromQuery] bool activeOnly = false)
        {
            StartProcessing();
            try
            {
                var menuItems = await _menuService.GetAllAsync(activeOnly);
                var list = menuItems.ToList();
                return await SuccessAsync(new MenuItemListResponse
                {
                    MenuItems = list,
                    TotalCount = list.Count
                }, "Menu items retrieved successfully");
            }
            catch (Exception ex) { return await HandleExceptionAsync<MenuItemListResponse>(ex, "Failed to get menu items"); }
            finally { StopProcessing(); }
        }

        [HttpGet("key/{menuKey}")]
        [AuthorizeByRoleLevel(RoleLevel.Admin, RoleLevel.SuperAdmin)]
        public async Task<ActionResult<GenericResponseModel>> GetMenuItemByKey(string menuKey)
        {
            StartProcessing();
            try
            {
                var menu = await _menuService.GetByMenuKeyAsync(menuKey);
                if (menu == null)
                    return NotFound<GenericResponseModel>("Menu not found");

                return await SuccessAsync(new GenericResponseModel { Data = menu }, "Menu retrieved successfully");
            }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to get menu"); }
            finally { StopProcessing(); }
        }

        [HttpPost("create")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        public async Task<ActionResult<GenericResponseModel>> CreateMenuItem([FromBody] CreateMenuItemRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<GenericResponseModel>(request.GetValidationErrors());

                request.CreatedBy = User.GetUsername() ?? "system";

                var menu = await _menuService.CreateAsync(request);
                return await SuccessAsync(new GenericResponseModel { Data = menu }, "Menu created successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to create menu"); }
            finally { StopProcessing(); }
        }

        [HttpPut("update")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        public async Task<ActionResult<GenericResponseModel>> UpdateMenuItem([FromBody] UpdateMenuItemRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<GenericResponseModel>(request.GetValidationErrors());

                var existingMenu = await _menuService.GetByMenuKeyAsync(request.MenuKey);
                if (existingMenu == null)
                    return NotFound<GenericResponseModel>("Menu not found");

                request.Id = existingMenu.MenuId;
                request.UpdatedBy = User.GetUsername() ?? "system";

                var success = await _menuService.UpdateAsync(request);
                if (!success)
                    return Error<GenericResponseModel>("Failed to update menu", "UPDATE_FAILED");

                var menu = await _menuService.GetByMenuKeyAsync(request.MenuKey);
                return await SuccessAsync(new GenericResponseModel { Data = menu }, "Menu updated successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to update menu"); }
            finally { StopProcessing(); }
        }

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
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to delete menu"); }
            finally { StopProcessing(); }
        }
    }
}
