using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Core.Logging;
using Nghex.Web.AspNetCore.Models;
using System.Diagnostics;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Controllers
{
    /// <summary>
    /// Base controller for all API controllers - only contains base functionality
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Logging Service
        /// </summary>
        protected readonly ILogging _loggingService;

        /// <summary>
        /// Stopwatch
        /// </summary>
        private readonly Stopwatch? _stopwatch;

        /// <summary>
        /// Performance tracking options
        /// </summary>
        private readonly PerformanceTrackingOptions _options;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggingService">Logging Service</param>
        /// <param name="options">Options</param>
        protected BaseController(ILogging loggingService, IOptions<PerformanceTrackingOptions> options)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _options = options?.Value ?? new PerformanceTrackingOptions();
            
            // Create Stopwatch if enabled
            if (_options.Enabled)
            {
                _stopwatch = new Stopwatch();
            }
        }

        /// <summary>
        /// Start processing timer
        /// </summary>
        protected void StartProcessing()
        {
            _stopwatch?.Restart();
        }

        /// <summary>
        /// Stop processing timer
        /// </summary>
        protected void StopProcessing()
        {
            _stopwatch?.Stop();
        }

        /// <summary>
        /// Get processing time elapsed
        /// </summary>
        /// <returns>Elapsed time</returns>
        protected TimeSpan GetProcessingTime()
        {
            return _stopwatch?.Elapsed ?? TimeSpan.Zero;
        }

        /// <summary>
        /// Create success response with BaseResponseModel
        /// </summary>
        /// <typeparam name="T">Response model type</typeparam>
        /// <param name="data">Response data</param>
        /// <param name="message">Success message</param>
        /// <returns>Success response</returns>
        protected async Task<ActionResult<T>> SuccessAsync<T>(T data, string? message = null) where T : BaseResponseModel
        {
            if (data.BaseResponse == null)
            {
                throw new InvalidOperationException("BaseResponse cannot be null. Ensure BaseResponseModel constructor is called.");
            }

            var processingTime = GetProcessingTime();
            
            // Set processing time if enabled
            if (_options.IncludeProcessingTimeInResponse)
            {
                data.SetProcessingTime(processingTime);
            }
            
            data.BaseResponse.Success = true;
            data.BaseResponse.Message = message ?? "Operation completed successfully";
            
            // Log slow requests if enabled
            if (_options.LogSlowRequests && processingTime.TotalMilliseconds >= _options.MinProcessingTimeMs)
            {
                await LogSlowRequestAsync(processingTime, message);
            }
            
            return Ok(data);
        }

        /// <summary>
        /// Create error response with BaseResponseModel
        /// </summary>
        /// <typeparam name="T">Response model type</typeparam>
        /// <param name="message">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="errorDetails">Error details</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>Error response</returns>
        protected ActionResult<T> Error<T>(string message, string? errorCode = null, string? errorDetails = null, int statusCode = 400) where T : BaseResponseModel, new()
        {
            var response = BaseResponseModel.CreateError<T>(message, errorCode, errorDetails);
            
            // Set processing time if enabled
            if (_options.IncludeProcessingTimeInResponse)
            {
                response.SetProcessingTime(GetProcessingTime());
            }
            
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Create validation error response
        /// </summary>
        /// <typeparam name="T">Response model type</typeparam>
        /// <param name="validationErrors">Validation errors</param>
        /// <returns>Validation error response</returns>
        protected ActionResult<T> ValidationError<T>(IEnumerable<string> validationErrors) where T : BaseResponseModel, new()
        {
            var errorMessage = string.Join("; ", validationErrors);
            var response = BaseResponseModel.CreateError<T>(errorMessage, "VALIDATION_ERROR");
            
            // Set processing time if enabled
            if (_options.IncludeProcessingTimeInResponse)
            {
                response.SetProcessingTime(GetProcessingTime());
            }
            
            return BadRequest(response);
        }

        /// <summary>
        /// Create not found response
        /// </summary>
        /// <typeparam name="T">Response model type</typeparam>
        /// <param name="message">Not found message</param>
        /// <returns>Not found response</returns>
        protected ActionResult<T> NotFound<T>(string message = "Resource not found") where T : BaseResponseModel, new()
        {
            var response = BaseResponseModel.CreateError<T>(message, "NOT_FOUND");
            
            // Set processing time if enabled
            if (_options.IncludeProcessingTimeInResponse)
            {
                response.SetProcessingTime(GetProcessingTime());
            }
            
            return NotFound(response);
        }

        /// <summary>
        /// Create unauthorized response
        /// </summary>
        /// <typeparam name="T">Response model type</typeparam>
        /// <param name="message">Unauthorized message</param>
        /// <returns>Unauthorized response</returns>
        protected ActionResult<T> Unauthorized<T>(string message = "Unauthorized access") where T : BaseResponseModel, new()
        {
            var response = BaseResponseModel.CreateError<T>(message, "UNAUTHORIZED");
            
            // Set processing time if enabled
            if (_options.IncludeProcessingTimeInResponse)
            {
                response.SetProcessingTime(GetProcessingTime());
            }
            
            return Unauthorized(response);
        }

        /// <summary>
        /// Create forbidden response
        /// </summary>
        /// <typeparam name="T">Response model type</typeparam>
        /// <param name="message">Forbidden message</param>
        /// <returns>Forbidden response</returns>
        protected ActionResult<T> Forbidden<T>(string message = "Access forbidden") where T : BaseResponseModel, new()
        {
            var response = BaseResponseModel.CreateError<T>(message, "FORBIDDEN");
            
            // Set processing time if enabled
            if (_options.IncludeProcessingTimeInResponse)
            {
                response.SetProcessingTime(GetProcessingTime());
            }
            
            return StatusCode(403, response);
        }

        /// <summary>
        /// Create internal server error response
        /// </summary>
        /// <typeparam name="T">Response model type</typeparam>
        /// <param name="message">Error message</param>
        /// <param name="errorDetails">Error details</param>
        /// <returns>Internal server error response</returns>
        protected ActionResult<T> InternalServerError<T>(string message = "Internal server error", string? errorDetails = null) where T : BaseResponseModel, new()
        {
            var response = BaseResponseModel.CreateError<T>(message, "INTERNAL_SERVER_ERROR", errorDetails);
            
            // Set processing time if enabled
            if (_options.IncludeProcessingTimeInResponse)
            {
                response.SetProcessingTime(GetProcessingTime());
            }
            
            return StatusCode(500, response);
        }

        /// <summary>
        /// Handle exception và log error
        /// </summary>
        /// <typeparam name="T">Response model type</typeparam>
        /// <param name="ex">Exception</param>
        /// <param name="message">Custom error message</param>
        /// <returns>Internal server error response</returns>
        protected async Task<ActionResult<T>> HandleExceptionAsync<T>(Exception ex, string? message = null) where T : BaseResponseModel, new()
        {
            await _loggingService.LogErrorAsync(
                message ?? "An error occurred",
                ex,
                source: GetType().Name,
                module: typeof(T).Name,
                action: typeof(T).Name + "HandleException"
            );
            return InternalServerError<T>(message ?? "An error occurred", ex.Message);
        }

        /// <summary>
        /// Log slow request
        /// </summary>
        /// <param name="processingTime">Processing time</param>
        /// <param name="message">Request message</param>
        private async Task LogSlowRequestAsync(TimeSpan processingTime, string? message)
        {
            var logLevel = _options.SlowRequestLogLevel.ToLower() switch
            {
                "information" => Nghex.Core.Enum.LogLevel.Information,
                "warning" => Nghex.Core.Enum.LogLevel.Warning,
                "error" => Nghex.Core.Enum.LogLevel.Error,
                _ => Nghex.Core.Enum.LogLevel.Warning
            };

            await _loggingService.LogAsync(
                logLevel,
                $"Slow API Request: {processingTime.TotalMilliseconds}ms - {message ?? "Unknown operation"}",
                source: GetType().Name,
                module: "API",
                action: "SlowRequest"
            );
        }
    }
}