using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Models.HttpLog
{
 
    /// <summary>
    /// Request log model
    /// </summary>
    public class HttpRequestLogModel : BaseLogModel
    {
        /// <summary>
        /// HTTP method
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Request path
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Client IP address
        /// </summary>
        public string? ClientIpAddress { get; set; }

        /// <summary>
        /// User agent
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Request body (optional)
        /// </summary>
        public object? RequestBody { get; set; }
        
        /// <summary>
        /// Requested at
        /// </summary>
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpRequestLogModel()
        {
            Level = "Information";
        }
    }

 }
