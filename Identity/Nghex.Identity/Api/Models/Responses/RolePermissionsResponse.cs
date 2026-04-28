namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for role permissions
    /// </summary>
    public class RolePermissionsResponse
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
        /// List of permissions
        /// </summary>
        public List<PermissionResponse> Permissions { get; set; } = [];
    }
}
