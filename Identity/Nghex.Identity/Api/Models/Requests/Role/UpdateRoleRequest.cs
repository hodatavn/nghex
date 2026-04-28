using System.ComponentModel.DataAnnotations;

namespace Nghex.Identity.Api.Models.Requests
{
    /// <summary>
    /// Request model for updating a role
    /// </summary>
    public class UpdateRoleRequest : CreateRoleRequest
    {
        /// <summary>
        /// Role ID
        /// </summary>
        [Required(ErrorMessage = "Role ID is required")]
        public long Id { get; set; }

        public string UpdatedBy { get; set; } = string.Empty;
    }
}
