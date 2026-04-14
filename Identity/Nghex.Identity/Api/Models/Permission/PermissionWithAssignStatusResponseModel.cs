namespace Nghex.Identity.Api.Models.Permission
{
    /// <summary>
    /// Response model for permission with assign status
    /// </summary>
    public class PermissionWithAssignStatusResponseModel
    {
        /// <summary>
        /// Permission ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Permission code
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Permission name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Is assigned
        /// </summary>
        public bool IsAssigned { get; set; }
    }
}