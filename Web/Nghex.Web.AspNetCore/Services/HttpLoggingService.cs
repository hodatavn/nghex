using Microsoft.Extensions.Logging;
using Nghex.Web.AspNetCore.Models.HttpLog;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Services
{
    /// <summary>
    /// Service to create log models for HTTP requests and responses
    /// </summary>
    public class HttpLoggingService
    {
        /// <summary>
        /// Tạo log model cho API request
        /// </summary>
        /// <typeparam name="T">Request model type</typeparam>
        /// <param name="request">Request model</param>
        /// <param name="method">HTTP method</param>
        /// <param name="path">Request path</param>
        /// <returns>API request log model</returns>
        public HttpRequestLogModel CreateRequestLog<T>(T request, string method, string path) where T : BaseRequestModel
        {
            return new HttpRequestLogModel
            {
                Method = method,
                Path = path,
                RequestId = request.BaseRequest?.RequestId ?? string.Empty,
                UserId = request.BaseRequest?.UserId,
                // Username = request.BaseRequest?.Username,
                ClientIpAddress = request.BaseRequest?.IpAddress ?? string.Empty,
                UserAgent = request.BaseRequest?.UserAgent ?? string.Empty,
                RequestedAt = request.BaseRequest?.RequestedAt ?? DateTime.UtcNow,
                RequestBody = request
            };
        }

        /// <summary>
        /// Tạo log model cho API response
        /// </summary>
        /// <typeparam name="T">Response model type</typeparam>
        /// <param name="response">Response model</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>API response log model</returns>
        public HttpResponseLogModel CreateResponseLog<T>(T response, int statusCode) where T : BaseResponseModel
        {
            return new HttpResponseLogModel
            {
                Success = response.BaseResponse?.Success ?? false,
                StatusCode = statusCode,
                RequestId = response.BaseResponse?.RequestId,
                ProcessingTimeMs = response.BaseResponse?.ProcessingTimeMs ?? 0,
                ErrorCode = response.BaseResponse?.ErrorCode,
                ErrorDetails = response.BaseResponse?.ErrorDetails,
                Message = response.BaseResponse?.Message ?? string.Empty,
                RespondedAt = response.BaseResponse?.RespondedAt ?? DateTime.UtcNow,
                ResponseBody = response
            };
        }

        /// <summary>
        /// Tạo log model cho API error
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Error message</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="context">Error context</param>
        /// <returns>API error log model</returns>
        public HttpErrorLogModel CreateErrorLog(Exception ex, string message, string? requestId = null, string? context = null)
        {
            return new HttpErrorLogModel
            {
                Message = message,
                RequestId = requestId,
                ExceptionType = ex.GetType().Name,
                ExceptionMessage = ex.Message,
                StackTrace = ex.StackTrace,
                Context = context,
                LoggedAt = DateTime.UtcNow
            };
        }
    }
}
