using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Requests
{
    /// <summary>
    /// Request model for assigning permissions to a menu item
    /// </summary>
    public class MenuItemPermissionRequest : BaseRequestModel
    {
        /// <summary>
        /// Menu key
        /// </summary>
        [Required(ErrorMessage = "Menu key is required")]
        [StringLength(100, ErrorMessage = "Menu key cannot exceed 100 characters")]
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>
        /// Permission code
        /// </summary>
        [Required(ErrorMessage = "Permission code is required")]
        [StringLength(100, ErrorMessage = "Permission code cannot exceed 100 characters")]
        public string PermissionCode { get; set; } = string.Empty;

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(MenuKey) &&
                   !string.IsNullOrWhiteSpace(PermissionCode) &&
                   MenuKey.Length <= 100 &&
                   PermissionCode.Length <= 100;
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();

            if (string.IsNullOrWhiteSpace(MenuKey))
                errors.Add("Menu key is required");
            else if (MenuKey.Length > 100)
                errors.Add("Menu key cannot exceed 100 characters");

            if (string.IsNullOrWhiteSpace(PermissionCode))
                errors.Add("Permission code is required");
            else if (PermissionCode.Length > 100)
                errors.Add("Permission code cannot exceed 100 characters");

            return errors;
        }
    }
}
