using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.MenuItemPermission
{
    /// <summary>
    /// Request model for removing permissions from a menu item using permission codes
    /// </summary>
    public class RemoveMenuPermissionsRequestModel : BaseRequestModel
    {
        /// <summary>
        /// Menu key
        /// </summary>
        [Required(ErrorMessage = "Menu key is required")]
        [StringLength(100, ErrorMessage = "Menu key cannot exceed 100 characters")]
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>
        /// List of permission codes to remove
        /// </summary>
        [Required(ErrorMessage = "Permission codes are required")]
        public List<string> PermissionCodes { get; set; } = new();

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(MenuKey) &&
                   MenuKey.Length <= 100 &&
                   PermissionCodes != null &&
                   PermissionCodes.Any();
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();
            
            if (string.IsNullOrWhiteSpace(MenuKey))
                errors.Add("Menu key is required");
            else if (MenuKey.Length > 100)
                errors.Add("Menu key cannot exceed 100 characters");

            if (PermissionCodes == null || !PermissionCodes.Any())
                errors.Add("At least one permission code is required");

            if (PermissionCodes != null && PermissionCodes.Any(string.IsNullOrWhiteSpace))
                errors.Add("All permission codes must be non-empty");

            return errors;
        }
    }
}
