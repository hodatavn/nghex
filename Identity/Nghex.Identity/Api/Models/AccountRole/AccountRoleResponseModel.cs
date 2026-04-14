using Nghex.Web.AspNetCore.Models.Base;
using Nghex.Identity.Api.Models.Role;

namespace Nghex.Identity.Api.Models.AccountRole
{
    /// <summary>
    /// Response model for account roles
    /// </summary>
    public class AccountRoleResponseModel : BaseResponseModel
    {
        /// <summary>
        /// Account ID
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// Account username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// List of roles
        /// </summary>
        public List<RoleResponseModel> Roles { get; set; } = new();

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}




