using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;
using Nghex.Core.Helper;

namespace Nghex.Identity.Api.Models.Role
{
    /// <summary>
    /// Request model for updating a role
    /// </summary>
    public class UpdateRoleRequestModel : CreateRoleRequestModel
    {
        /// <summary>
        /// Role ID
        /// </summary>
        [Required(ErrorMessage = "Role ID is required")]
        public long Id { get; set; }

    }
}




