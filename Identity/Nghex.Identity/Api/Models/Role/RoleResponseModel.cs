using Nghex.Core.Enum;
using Nghex.Identity.Enum;

namespace Nghex.Identity.Api.Models.Role
{
    /// <summary>
    /// Response model for Role
    /// </summary>
    public class RoleResponseModel 
    {
        /// <summary>
        /// Role ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// Role code
        /// </summary>
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// Role name
        /// </summary>
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Role description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Role level
        /// </summary>
        public RoleLevel RoleLevel { get; set; }

        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; }

    }
}




