using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Requests
{
    /// <summary>
    /// Request model for creating a role
    /// </summary>
    public class CreateRoleRequest : BaseRequestModel
    {
        /// <summary>
        /// Role code
        /// </summary>
        [Required(ErrorMessage = "Role code is required")]
        [StringLength(50, ErrorMessage = "Role code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Role name
        /// </summary>
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Role description
        /// </summary>
        [StringLength(500, ErrorMessage = "Role description cannot exceed 500 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Role level
        /// </summary>
        [Required(ErrorMessage = "Role level is required")]
        public int RoleLevel { get; set; }

        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Code) &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   Code.Length <= 50 &&
                   Name.Length <= 100 &&
                   (Description == null || Description.Length <= 500);
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();

            if (string.IsNullOrWhiteSpace(Code))
                errors.Add("Role code is required");
            else if (Code.Length > 50)
                errors.Add("Role code cannot exceed 50 characters");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(Code, @"^[a-zA-Z0-9_]+$"))
                errors.Add("Role code can only contain letters, numbers, and underscores");

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Role name is required");
            else if (Name.Length > 100)
                errors.Add("Role name cannot exceed 100 characters");

            if (!string.IsNullOrWhiteSpace(Description) && Description.Length > 500)
                errors.Add("Role description cannot exceed 500 characters");

            return errors;
        }
    }
}
