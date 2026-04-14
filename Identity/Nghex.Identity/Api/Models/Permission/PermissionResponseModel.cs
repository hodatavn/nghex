
namespace Nghex.Identity.Api.Models.Permission
{
    /// <summary>
    /// Response model for Permission
    /// </summary>
    public class PermissionResponseModel
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
        /// Permission description
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Plugin name
        /// </summary>
        public string? PluginName { get; set; }

        /// <summary>
        /// Module
        /// </summary>
        public string? Module { get; set; }

        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; }

    }
}
