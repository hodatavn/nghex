using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for list of roles
    /// </summary>
    public class RoleListResponse : BaseResponseModel
    {
        /// <summary>
        /// List of roles
        /// </summary>
        public List<RoleResponse> Roles { get; set; } = [];

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}
