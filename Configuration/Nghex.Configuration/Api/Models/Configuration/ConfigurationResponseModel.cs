using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Configuration.Api.Models
{
    /// <summary>
    /// Response model for list of roles
    /// </summary>
    public class ConfigurationResponseModel 
    {
        /// <summary>
        /// Configuration ID
        /// </summary>
        public long ConfigurationId { get; set; }
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; } = string.Empty;
        /// <summary>
        /// Value
        /// </summary>
        public string? Value { get; set; }
        /// <summary>
        /// Data type
        /// </summary>
        public string DataType { get; set; } = string.Empty;
        /// <summary>
        /// Default value
        /// </summary>
        public string? DefaultValue { get; set; }
        /// <summary>
        /// Module
        /// </summary>
        public string? Module { get; set; }
        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Is system config
        /// </summary>
        public bool IsSystemConfig { get; set; } = false;
        /// <summary>
        /// Is editable
        /// </summary>
        public bool IsEditable { get; set; } = true;
        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
