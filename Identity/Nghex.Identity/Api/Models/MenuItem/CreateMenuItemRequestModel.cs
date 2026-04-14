using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.MenuItem
{
    /// <summary>
    /// Request model for creating a menu item
    /// </summary>
    public class CreateMenuItemRequestModel : BaseRequestModel
    {
        /// <summary>
        /// Menu key (unique)
        /// </summary>
        [Required(ErrorMessage = "Menu key is required")]
        [StringLength(100, ErrorMessage = "Menu key cannot exceed 100 characters")]
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>
        /// Parent menu key (optional)
        /// </summary>
        [StringLength(100, ErrorMessage = "Parent key cannot exceed 100 characters")]
        public string? ParentKey { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Route (optional)
        /// </summary>
        [StringLength(500, ErrorMessage = "Route cannot exceed 500 characters")]
        public string? Route { get; set; }

        /// <summary>
        /// Icon (optional)
        /// </summary>
        [StringLength(200, ErrorMessage = "Icon cannot exceed 200 characters")]
        public string? Icon { get; set; }

        /// <summary>
        /// Plugin name (optional)
        /// </summary>
        [StringLength(100, ErrorMessage = "Plugin name cannot exceed 100 characters")]
        public string? PluginName { get; set; }

        /// <summary>
        /// Permission prefix (optional)
        /// </summary>
        [StringLength(100, ErrorMessage = "Permission prefix cannot exceed 100 characters")]
        public string? PermissionPrefix { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(MenuKey) &&
                   MenuKey.Length <= 100 &&
                   !string.IsNullOrWhiteSpace(Title) &&
                   Title.Length <= 200 &&
                   (ParentKey == null || ParentKey.Length <= 100) &&
                   (Route == null || Route.Length <= 500) &&
                   (Icon == null || Icon.Length <= 200) &&
                   (PluginName == null || PluginName.Length <= 100) &&
                   (PermissionPrefix == null || PermissionPrefix.Length <= 100);
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();
            
            if (string.IsNullOrWhiteSpace(MenuKey))
                errors.Add("Menu key is required");
            else if (MenuKey.Length > 100)
                errors.Add("Menu key cannot exceed 100 characters");

            if (string.IsNullOrWhiteSpace(Title))
                errors.Add("Title is required");
            else if (Title.Length > 200)
                errors.Add("Title cannot exceed 200 characters");

            if (ParentKey != null && ParentKey.Length > 100)
                errors.Add("Parent key cannot exceed 100 characters");

            if (Route != null && Route.Length > 500)
                errors.Add("Route cannot exceed 500 characters");

            if (Icon != null && Icon.Length > 200)
                errors.Add("Icon cannot exceed 200 characters");

            if (PluginName != null && PluginName.Length > 100)
                errors.Add("Plugin name cannot exceed 100 characters");

            if (PermissionPrefix != null && PermissionPrefix.Length > 100)
                errors.Add("Permission prefix cannot exceed 100 characters");

            return errors;
        }
    }
}




