using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for account roles
    /// </summary>
    public class AccountRoleResponse : BaseResponseModel
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
        public List<RoleResponse> Roles { get; set; } = new();

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}
