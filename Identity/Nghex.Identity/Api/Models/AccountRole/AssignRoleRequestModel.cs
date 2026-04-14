using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.AccountRole
{
    /// <summary>
    /// Request model for assigning roles to an account
    /// </summary>
    public class AssignRoleRequestModel : BaseRequestModel
    {
        /// <summary>
        /// Account ID
        /// </summary>
        [Required(ErrorMessage = "Account ID is required")]
        public long AccountId { get; set; }

        /// <summary>
        /// List of role IDs to assign
        /// </summary>
        [Required(ErrorMessage = "Role IDs are required")]
        public List<long> RoleIds { get; set; } = new();

        public override bool IsValid()
        {
            return base.IsValid() &&
                   AccountId > 0 &&
                   RoleIds != null &&
                   RoleIds.Count > 0;
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();
            
            if (AccountId <= 0)
                errors.Add("Account ID must be greater than 0");

            if (RoleIds == null || RoleIds.Count == 0)
                errors.Add("At least one role ID is required");

            if (RoleIds != null && RoleIds.Any(r => r <= 0))
                errors.Add("All role IDs must be greater than 0");

            return errors;
        }
    }
}




