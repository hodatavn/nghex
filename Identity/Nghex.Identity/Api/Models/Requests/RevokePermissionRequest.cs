using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Requests
{
    /// <summary>
    /// Request model for revoking permissions from a role
    /// </summary>
    public class RevokePermissionRequest : BaseRequestModel
    {
        /// <summary>
        /// Role ID
        /// </summary>
        [Required(ErrorMessage = "Role ID is required")]
        public long RoleId { get; set; }

        /// <summary>
        /// List of permission IDs to revoke
        /// </summary>
        [Required(ErrorMessage = "Permission IDs are required")]
        public List<long> PermissionIds { get; set; } = new();

        public override bool IsValid()
        {
            return base.IsValid() &&
                   RoleId > 0 &&
                   PermissionIds != null &&
                   PermissionIds.Any();
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();

            if (RoleId <= 0)
                errors.Add("Role ID must be greater than 0");

            if (PermissionIds == null || !PermissionIds.Any())
                errors.Add("At least one permission ID is required");

            if (PermissionIds != null && PermissionIds.Any(p => p <= 0))
                errors.Add("All permission IDs must be greater than 0");

            return errors;
        }
    }
}
