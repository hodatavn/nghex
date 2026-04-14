using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.MenuItemPermission
{
    /// <summary>
    /// Request model for removing permissions from a menu item (use-case: (menuKey, list&lt;permissionIds&gt;))
    /// </summary>
    public class RemovePermissionsFromMenuItemRequestModel : BaseRequestModel
    {
        /// <summary>
        /// Menu key
        /// </summary>
        [Required(ErrorMessage = "Menu key is required")]
        [StringLength(100, ErrorMessage = "Menu key cannot exceed 100 characters")]
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>
        /// List of permission IDs to remove
        /// </summary>
        [Required(ErrorMessage = "Permission IDs are required")]
        public List<long> PermissionIds { get; set; } = new();

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(MenuKey) &&
                   MenuKey.Length <= 100 &&
                   PermissionIds != null &&
                   PermissionIds.Any();
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();
            
            if (string.IsNullOrWhiteSpace(MenuKey))
                errors.Add("Menu key is required");
            else if (MenuKey.Length > 100)
                errors.Add("Menu key cannot exceed 100 characters");

            if (PermissionIds == null || !PermissionIds.Any())
                errors.Add("At least one permission ID is required");

            if (PermissionIds != null && PermissionIds.Any(p => p <= 0))
                errors.Add("All permission IDs must be greater than 0");

            return errors;
        }
    }
}




