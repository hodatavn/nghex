using System.ComponentModel.DataAnnotations;

namespace Nghex.Web.AspNetCore.Models.Base
{
    /// <summary>
    /// Base response model cho tất cả API responses
    /// </summary>
    public abstract class BaseResponseModel
    {
        /// <summary>
        /// Constructor to initialize default values
        /// </summary>
        protected BaseResponseModel()
        {
            BaseResponse = new BaseResponse();
        }

        /// <summary>
        /// Base response
        /// </summary>
        public BaseResponse BaseResponse { get; private set; }
        
        /// <summary>
        /// Create success response
        /// </summary>
        /// <param name="message">Success message</param>
        /// <param name="requestId">Request ID</param>
        /// <returns>Success response</returns>
        public static T CreateSuccess<T>(string? message = null, string? requestId = null) where T : BaseResponseModel, new()
        {
            var response = new T();
            response.BaseResponse.Success = true;
            response.BaseResponse.Message = message ?? "Operation completed successfully";
            response.BaseResponse.RequestId = requestId;
            return response;
        }

        /// <summary>
        /// Create error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="errorDetails">Error details</param>
        /// <param name="requestId">Request ID</param>
        /// <returns>Error response</returns>
        public static T CreateError<T>(string message, string? errorCode = null, string? errorDetails = null, string? requestId = null) where T : BaseResponseModel, new()
        {
            var response = new T();
            response.BaseResponse.Success = false;
            response.BaseResponse.Message = message;
            response.BaseResponse.ErrorCode = errorCode;
            response.BaseResponse.ErrorDetails = errorDetails;
            response.BaseResponse.RequestId = requestId;
            return response;
        }

        /// <summary>
        /// Set processing time from start time
        /// </summary>
        /// <param name="startTime">Start time</param>
        public void SetProcessingTime(DateTime startTime)
        {
            if (BaseResponse == null)
            {
                throw new InvalidOperationException("BaseResponse cannot be null. Ensure BaseResponseModel constructor is called.");
            }
            BaseResponse.ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        }

        /// <summary>
        /// Set processing time from elapsed time
        /// </summary>
        /// <param name="elapsed">Elapsed time</param>
        public void SetProcessingTime(TimeSpan elapsed)
        {
            if (BaseResponse == null)
            {
                throw new InvalidOperationException("BaseResponse cannot be null. Ensure BaseResponseModel constructor is called.");
            }
            BaseResponse.ProcessingTimeMs = (long)elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// Add metadata
        /// </summary>
        /// <param name="key">Metadata key</param>
        /// <param name="value">Metadata value</param>
        public void AddMetadata(string key, object value)
        {
            if (BaseResponse == null)
            {
                throw new InvalidOperationException("BaseResponse cannot be null. Ensure BaseResponseModel constructor is called.");
            }
            BaseResponse.Metadata ??= new Dictionary<string, object>();
            BaseResponse.Metadata[key] = value;
        }
    }

    /// <summary>
    /// Base response object containing common response metadata
    /// </summary>
    public class BaseResponse
    {
        /// <summary>
        /// Constructor to initialize default values
        /// </summary>
        public BaseResponse()
        {
            ResponseId = Guid.NewGuid().ToString();
            Success = true;
            Message = string.Empty;
            RespondedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Response ID for tracking
        /// </summary>
        [StringLength(100)]
        public string ResponseId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Request ID corresponding to the request
        /// </summary>
        [StringLength(100)]
        public string? RequestId { get; set; }

        /// <summary>
        /// Success status
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Response message
        /// </summary>
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Error code if there is an error
        /// </summary>
        [StringLength(50)]
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Error details if there is an error
        /// </summary>
        public string? ErrorDetails { get; set; }

        /// <summary>
        /// Timestamp when the response is created
        /// </summary>
        public DateTime RespondedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Processing time in milliseconds
        /// </summary>
        public long ProcessingTimeMs { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
