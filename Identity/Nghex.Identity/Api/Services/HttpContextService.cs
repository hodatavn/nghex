using Microsoft.AspNetCore.Http;
using System.Net;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Services
{
    public interface IHttpContextService
    {
        string GetClientIpAddress();
        string GetUserAgent();
        string GetRequestId();
        long? GetUserId();
        string? GetUsername();
    }
    /// <summary>
    /// Service to extract information from HTTP context
    /// </summary>
    public class HttpContextService : IHttpContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Get client IP address
        /// </summary>
        /// <returns>Client IP address</returns>
        public string GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            var ipAddress = IPAddress.Loopback;
            if (context == null) return $"{ipAddress}";
            try
            {
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    if(IPAddress.TryParse(forwardedFor.Split(',')[0].Trim(), out var ip))
                        ipAddress = ip;
                }
                else
                {
                    var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(realIp))
                    {
                        if(IPAddress.TryParse(realIp, out var ip))
                            ipAddress = ip;
                    }
                    else {
                        ipAddress = context.Connection.RemoteIpAddress;
                        ipAddress ??= IPAddress.Loopback;
                    }
                }
            }
            catch{
                ipAddress = IPAddress.Loopback;
            }
            return $"{ipAddress.MapToIPv4()}";
        }

        /// <summary>
        /// Get user agent
        /// </summary>
        /// <returns>User agent string</returns>
        public string GetUserAgent()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return "Unknown";
                var userAgent = context.Request.Headers.UserAgent.ToString() ;
                return !string.IsNullOrWhiteSpace(userAgent) ? userAgent.Trim() : "Unknown";
            }
            catch{
                return "Unknown";  
            }
        }

        /// <summary>
        /// Get request ID from headers
        /// </summary>
        /// <returns>Request ID</returns>
        public string GetRequestId()
        {
            var requestId = $"{Guid.NewGuid()}";
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return requestId;

                return context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? 
                       context.Request.Headers["Request-ID"].FirstOrDefault() ?? requestId;
            }
            catch
            {
                // Ignore errors
            }
            return requestId;
        }

        /// <summary>
        /// Get user ID from claims
        /// </summary>
        /// <returns>User ID</returns>
        public long? GetUserId()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context?.User == null) return null;

                var userIdClaim =
                    context.User.FindFirst("account_id") ??
                    context.User.FindFirst("user_id") ??
                    context.User.FindFirst("sub") ??
                    context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
                    return userId;
            }
            catch
            {
                // Ignore errors
            }
            return null;
        }

        /// <summary>
        /// Get username from claims
        /// </summary>
        /// <returns>Username</returns>
        public string? GetUsername()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context?.User == null) return null;

                return context.User.FindFirst("username")?.Value ?? 
                       context.User.FindFirst("name")?.Value ?? 
                       context.User.Identity?.Name;
            }
            catch
            {
                // Ignore errors
            }
            return null;
        }

        /// <summary>
        /// Populate request model with information from HTTP context
        /// </summary>
        /// <typeparam name="T">Request model type</typeparam>
        /// <param name="request">Request model</param>
        /// <returns>Populated request model</returns>
        public T PopulateRequestModel<T>(T request) where T : BaseRequestModel
        {
            if (request.BaseRequest == null)
            {
                throw new InvalidOperationException("BaseRequest cannot be null. Ensure BaseRequestModel constructor is called.");
            }
            
            request.BaseRequest.RequestId = GetRequestId();
            request.BaseRequest.UserId = GetUserId();
            // request.BaseRequest.Username = GetUsername() ?? string.Empty;
            request.BaseRequest.IpAddress = GetClientIpAddress();
            request.BaseRequest.UserAgent = GetUserAgent();
            request.BaseRequest.RequestedAt = DateTime.UtcNow;
            return request;
        }
    }
}
