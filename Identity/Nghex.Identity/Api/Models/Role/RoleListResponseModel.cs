using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Role
{
    /// <summary>
    /// Response model for list of roles
    /// </summary>
    public class RoleListResponseModel : BaseResponseModel
    {
        /// <summary>
        /// List of roles
        /// </summary>
        public List<RoleResponseModel> Roles { get; set; } = [];

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}




