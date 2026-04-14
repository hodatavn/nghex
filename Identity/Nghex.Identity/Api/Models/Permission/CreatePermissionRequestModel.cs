using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;
using Nghex.Core.Helper;

namespace Nghex.Identity.Api.Models.Permission
{
    /// <summary>
    /// Request model for creating a permission
    /// </summary>
    public class CreatePermissionRequestModel : BaseRequestModel
    {
        /// <summary>
        /// Permission code
        /// </summary>
        [Required(ErrorMessage = "Permission code is required")]
        [StringLength(100, ErrorMessage = "Permission code cannot exceed 100 characters")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Permission name
        /// </summary>
        [Required(ErrorMessage = "Permission name is required")]
        [StringLength(100, ErrorMessage = "Permission name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Module
        /// </summary>
        [StringLength(100, ErrorMessage = "Module cannot exceed 100 characters")]
        public string? Module { get; set; }

        /// <summary>
        /// Plugin name
        /// </summary>
        [StringLength(100, ErrorMessage = "Plugin name cannot exceed 100 characters")]
        public string? PluginName { get; set; }
        
        /// <summary>
        /// Permission description
        /// </summary>
        [StringLength(500, ErrorMessage = "Permission description cannot exceed 500 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Code) &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   Code.Length <= 100 &&
                   Name.Length <= 100 &&
                   (Module == null || Module.Length <= 100) &&
                   (Description == null || Description.Length <= 500);
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();
            
            if (string.IsNullOrWhiteSpace(Code))
                errors.Add("Permission code is required");
            else if (Code.Length > 100)
                errors.Add("Permission code cannot exceed 100 characters");
            else if (!ModelHelper.IsValidCode(Code))
                errors.Add("Permission code can only contain letters, numbers, and underscores");

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Permission name is required");
            else if (Name.Length > 100)
                errors.Add("Permission name cannot exceed 100 characters");

            if (!string.IsNullOrWhiteSpace(Module))
            {
                if (Module.Length > 100)
                    errors.Add("Module cannot exceed 100 characters");
                else if (!ModelHelper.IsValidModule(Module))
                    errors.Add("Module can only contain letters, numbers, underscores, and dots");
            }

            if (!string.IsNullOrWhiteSpace(Description) && Description.Length > 500)
                errors.Add("Permission description cannot exceed 500 characters");

            return errors;
        }
    }
}




