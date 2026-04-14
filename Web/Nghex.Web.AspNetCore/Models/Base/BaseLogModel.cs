using System.Text.Json.Serialization;

namespace Nghex.Web.AspNetCore.Models.Base
{
    /// <summary>
    /// Base log model cho tất cả API logs
    /// </summary>
    public abstract class BaseLogModel
    {
        /// <summary>
        /// Log level
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Log message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Request ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Logged at
        /// </summary>
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional properties
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Add property
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void AddProperty(string key, object value)
        {
            Properties[key] = value;
        }
    }

}
