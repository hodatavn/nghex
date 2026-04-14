
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;
using Nghex.Web.AspNetCore.Models.Base;
using Nghex.Logging.Api.Models;
using Nghex.Core.Enum;
using Nghex.Logging.Interfaces;
using LogLevel = Nghex.Core.Enum.LogLevel;

namespace Nghex.Logging.Api.Controllers
{
    /// <summary>
    /// Log Controller for testing and managing application logs
    /// </summary>
    [Route("api/[controller]")]
    public class LogController(
        ILoggingService loggingService,
        IOptions<PerformanceTrackingOptions> options,
        ILogQueryService? logQueryService = null) : BaseController(loggingService, options)
    {
        private readonly ILogQueryService? _logQueryService = logQueryService;
        /// <summary>
        /// Get list of available log levels
        /// </summary>
        [HttpGet("levels")]
        public IActionResult GetLogLevels()
        {
            try
            {
                var levels = Enum.GetValues(typeof(LogLevel))
                    .Cast<LogLevel>()
                    .Select(level => new LogLevelResponseModel
                    {
                        Name = level.ToString(),
                        Value = (int)level,
                        Description = GetLogLevelDescription(level)
                    })
                    .ToList();

                return Ok(new LogLevelListResponseModel
                {
                    Levels = levels,
                    Count = levels.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get logs by level
        /// </summary>
        [HttpPost("level")]
        public async Task<IActionResult> GetLogsByLevel([FromBody] GetLogsByLevelRequest request)
        {
            try
            {
                var errors = request.GetValidationErrors();
                if (errors.Any())
                    return ValidationError<LogListResponseModel>(errors).Result!;

                var logLevel = request.GetLogLevel();
                var levelValue = (int)logLevel;

                var logs = await _logQueryService!.GetLogsByLevelAsync(logLevel, request.Skip, request.Take);
                var logList = logs.ToList();
                var totalCount = await _logQueryService!.CountLogsByLevelAsync(logLevel);

                return Ok(new LogListResponseModel
                {
                    Logs = logList,
                    Count = logList.Count,
                    TotalCount = totalCount,
                    LevelName = request.LevelName,
                    LevelValue = levelValue,
                    Skip = request.Skip,
                    Take = request.Take
                });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to get logs by level: {request.LevelName}",
                    ex,
                    source: "LogController.GetLogsByLevel",
                    module: "Logging",
                    action: "GetLogsByLevel",
                    details: new { LevelName = request.LevelName }
                );
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Delete logs by list of ids
        /// </summary>
        /// <summary>Same policy as <c>AuthorizeByRoleLevel(SuperAdmin, Admin)</c> — see JWT registration.</summary>
        [Authorize(Policy = "RoleLevelPolicy_0_1")]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteLogs([FromBody] DeleteLogsRequestModel request)
        {
            try
            {
                var errors = request.GetValidationErrors();
                if (errors.Any())
                    return ValidationError<GenericResponseModel>(errors).Result!;

                var requestedIds = request.Ids.Distinct().ToList();
                var deletedIds = new List<long>();
                var failedIds = new List<long>();

                foreach (var logId in requestedIds)
                {
                    var ok = await _logQueryService!.DeleteLogAsync(logId);
                    if (ok) deletedIds.Add(logId);
                    else failedIds.Add(logId);
                }

                var response = BaseResponseModel.CreateSuccess<GenericResponseModel>("Delete operation completed");
                response.Data = new
                {
                    RequestedCount = requestedIds.Count,
                    DeletedCount = deletedIds.Count,
                    FailedCount = failedIds.Count,
                    DeletedIds = deletedIds,
                    FailedIds = failedIds
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Failed to delete logs",
                    ex,
                    source: "LogController.DeleteLogs",
                    module: "Logging",
                    action: "DeleteLogs",
                    details: new { request?.Ids }
                );
                return BadRequest(new { Message = ex.Message });
            }
        }

        // /// <summary>
        // /// Delete logs by list of ids (supports DELETE method for UI clients)
        // /// </summary>
        // [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
        // [HttpDelete("delete")]
        // public async Task<IActionResult> DeleteLogsByDeleteMethod([FromBody] DeleteLogsRequestModel request)
        // {
        //     // Reuse the same implementation as POST /delete
        //     return await DeleteLogs(request);
        // }

        /// <summary>
        /// Get description for log level
        /// </summary>
        private static string GetLogLevelDescription(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => "Detailed information for debugging",
                LogLevel.Information => "General information",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical error",
                _ => "Unknown log level"
            };
        }

        /// <summary>
        /// Get logs by user
        /// </summary>
        [HttpPost("user")]
        public async Task<IActionResult> GetLogsByUser([FromBody] GetLogsByUserRequest request)
        {
            try
            {
                var errors = request.GetValidationErrors();
                if (errors.Any())
                    return ValidationError<LogListResponseModel>(errors).Result!;

                var logs = await _logQueryService!.GetLogsByUserAsync(request.Username, request.Skip, request.Take);
                var logList = logs.ToList();
                var totalCount = await _logQueryService!.CountLogsByUserAsync(request.Username);

                return Ok(new LogListResponseModel
                {
                    Logs = logList,
                    Count = logList.Count,
                    TotalCount = totalCount,
                    Username = request.Username,
                    Skip = request.Skip,
                    Take = request.Take
                });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to get logs by user: {request.Username}",
                    ex,
                    source: "LogController.GetLogsByUser",
                    module: "Logging",
                    action: "GetLogsByUser",
                    details: new { Username = request.Username }
                );
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get logs by module
        /// </summary>
        [HttpPost("module")]
        public async Task<IActionResult> GetLogsByModule([FromBody] GetLogsByModuleRequest request)
        {
            try
            {
                var errors = request.GetValidationErrors();
                if (errors.Any())
                    return ValidationError<LogListResponseModel>(errors).Result!;

                var logs = await _logQueryService!.GetLogsByModuleAsync(request.Module, request.Skip, request.Take);
                var logList = logs.ToList();
                var totalCount = await _logQueryService!.CountLogsByModuleAsync(request.Module);

                return Ok(new LogListResponseModel
                {
                    Logs = logList,
                    Count = logList.Count,
                    TotalCount = totalCount,
                    Module = request.Module,
                    Skip = request.Skip,
                    Take = request.Take
                });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to get logs by module: {request.Module}",
                    ex,
                    source: "LogController.GetLogsByModule",
                    module: "Logging",
                    action: "GetLogsByModule",
                    details: new { Module = request.Module }
                );
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Search logs by keyword
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchLogs([FromBody] SearchLogsRequestModel request)
        {
            try
            {
                var errors = request.GetValidationErrors();
                if (errors.Any())
                    return ValidationError<LogListResponseModel>(errors).Result!;

                var logs = await _logQueryService!.SearchLogsAsync(request.Keyword, request.Skip, request.Take);
                var logList = logs.ToList();
                var totalCount = await _logQueryService!.CountSearchLogsAsync(request.Keyword);

                return Ok(new LogListResponseModel
                {
                    Logs = logList,
                    Count = logList.Count,
                    TotalCount = totalCount,
                    Keyword = request.Keyword,
                    Skip = request.Skip,
                    Take = request.Take
                });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to search logs: {request.Keyword}",
                    ex,
                    source: "LogController.SearchLogs",
                    module: "Logging",
                    action: "SearchLogs",
                    details: new { Keyword = request.Keyword }
                );
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get logs by date range
        /// </summary>
        [HttpPost("daterange")]
        public async Task<IActionResult> GetLogsByDateRange([FromBody] GetLogsByDateRangeRequest request)
        {
            try
            {

                var errors = request.GetValidationErrors();
                if (errors.Any())
                    return ValidationError<LogListResponseModel>(errors).Result!;
                

                var logs = await _logQueryService!.GetLogsByDateRangeAsync(request.FromDate, request.ToDate, request.Skip, request.Take);
                var logList = logs.ToList();
                var totalCount = await _logQueryService!.CountLogsByDateRangeAsync(request.FromDate, request.ToDate);

                return Ok(new LogListResponseModel
                {
                    Logs = logList,
                    Count = logList.Count,
                    TotalCount = totalCount,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    Skip = request.Skip,
                    Take = request.Take
                });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to get logs by date range: {request.FromDate} - {request.ToDate}",
                    ex,
                    source: "LogController.GetLogsByDateRange",
                    module: "Logging",
                    action: "GetLogsByDateRange",
                    details: new { FromDate = request.FromDate, ToDate = request.ToDate }
                );
                return Error<LogListResponseModel>("Failed to get logs by date range", ex.Message).Result!;
            }
        }

    }

}