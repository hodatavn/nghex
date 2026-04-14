using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Models.HttpLog
{
    /// <summary>
    /// HTTP response log model
    /// </summary>
    public class HttpResponseLogModel : BaseLogModel
    {
        /// <summary>
        /// Response success status
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Processing time in milliseconds
        /// </summary>
        public long ProcessingTimeMs { get; set; }

        /// <summary>
        /// Error code (if error)
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Error details (if error)
        /// </summary>
        public string? ErrorDetails { get; set; }

        /// <summary>
        /// Response body (optional)
        /// </summary>
        public object? ResponseBody { get; set; }

        /// <summary>
        /// Responded at
        /// </summary>
        public DateTime RespondedAt { get; set; } = DateTime.UtcNow;

    }

}
