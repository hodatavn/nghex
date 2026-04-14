using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Models.HttpLog
{
    /// <summary>
    /// API error log model
    /// </summary>
    public class HttpErrorLogModel : BaseLogModel
    {
        /// <summary>
        /// Exception type
        /// </summary>
        public string? ExceptionType { get; set; }

        /// <summary>
        /// Exception message
        /// </summary>
        public string? ExceptionMessage { get; set; }

        /// <summary>
        /// Stack trace
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Error context
        /// </summary>
        public string? Context { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpErrorLogModel()
        {
            Level = "Error";
        }
    }
}
