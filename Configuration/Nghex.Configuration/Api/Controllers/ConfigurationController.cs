using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Configuration.Api.Models;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;
using Nghex.Configuration.DTOs;
using Nghex.Core.Enum;
using Nghex.Identity.Enum;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Configuration.Services.Interfaces;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Configuration.Api.Controllers
{
    /// <summary>
    /// Controller quản lý configurations
    /// </summary>
    [ApiController]
    [Tags("System Configuration")]
    [Route("api/[controller]")]
    public class ConfigurationController(
        IConfigurationService configurationService, 
        ILoggingService loggingService, 
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IConfigurationService _configurationService = configurationService;

        /// <summary>
        /// Get all available data types for configuration as list of string
        /// </summary>
        /// <returns>The available data types as list of string</returns>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("data-types")]
        public ActionResult GetDataTypes()
        {
            try
            {
                var dataTypes = _configurationService.GetDataTypes()
                    .Select(type => new DataTypeResponseModel
                    {
                        Name = type,
                        Value = type
                    }).ToList();
                return Ok(new DataTypeListResponseModel
                {
                    DataTypes = dataTypes,
                    Count = dataTypes.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get all configurations
        /// </summary>
        /// <returns>The configurations</returns>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("all")]
        public async Task<ActionResult<ConfigurationListResponseModel>> GetAllConfigurations()
        {
            StartProcessing();

            try
            {
                var configurationDtos = await _configurationService.GetAllAsync(true);
                var configurationList = configurationDtos.Select(c => c.Adapt<ConfigurationResponseModel>()).ToList();

                var response = new ConfigurationListResponseModel
                {
                    Configurations = configurationList,
                    TotalCount = configurationList.Count
                };

                return await SuccessAsync(response, "Configurations retrieved successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<ConfigurationListResponseModel>(ex, "Failed to get configurations");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Create new configuration
        /// </summary>
        /// <param name="request">The request model</param>
        /// <returns>The response message</returns>
        [HttpPost("create")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        public async Task<ActionResult<GenericResponseModel>> CreateConfiguration([FromBody] CreateConfigurationRequest request)
        {
            StartProcessing();

            try
            {
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors);
                }

                // Map Request -> DTO
                var createDto = request.Adapt<CreateConfigurationDto>();
                createDto.CreatedBy = User.GetUsername() ?? "system";

                // Service handles DTO -> Entity mapping internally
                var configurationDto = await _configurationService.CreateAsync(createDto);

                // Map DTO -> Response
                var configurationResponse = configurationDto.Adapt<ConfigurationResponseModel>();

                var response = new GenericResponseModel
                {
                    Data = configurationResponse
                };

                return await SuccessAsync(response, "Configuration created successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (ArgumentException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to create configuration");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Update configuration
        /// </summary>
        /// <param name="request">The request model</param>
        /// <returns>The response message</returns>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpPut("update")]
        public async Task<ActionResult<GenericResponseModel>> UpdateConfiguration([FromBody] UpdateConfigurationRequest request)
        {
            StartProcessing();

            try
            {
                if (!request.IsValid())
                {
                    var errors = request.GetValidationErrors();
                    return ValidationError<GenericResponseModel>(errors);
                }

                // Map Request -> DTO
                var updateDto = request.Adapt<UpdateConfigurationDto>();
                updateDto.UpdatedBy = User.GetUsername() ?? "system";

                // Service handles DTO -> Entity mapping internally
                var success = await _configurationService.UpdateAsync(updateDto);
                if (!success)
                    return Error<GenericResponseModel>("Failed to update configuration", "UPDATE_FAILED");

                // Get updated configuration
                var configurationDto = await _configurationService.GetByIdAsync(updateDto.Id);
                var configurationResponse = configurationDto?.Adapt<ConfigurationResponseModel>();

                var response = new GenericResponseModel
                {
                    Data = configurationResponse
                };

                return await SuccessAsync(response, "Configuration updated successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION");
            }
            catch (ArgumentException ex)
            {
                return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to update configuration");
            }
            finally
            {
                StopProcessing();
            }
        }


        /// <summary>
        /// Get configurations by module
        /// </summary>
        /// <param name="module">The module of the configurations to get</param>
        /// <returns>The configurations</returns>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("module/{module}")]
        public async Task<ActionResult<ConfigurationListResponseModel>> GetByModule(string module)
        {
            StartProcessing();

            try
            {
                var configurationDtos = await _configurationService.GetByModuleAsync(module);
                var configurationList = configurationDtos.Select(c => c.Adapt<ConfigurationResponseModel>()).ToList();

                var response = new ConfigurationListResponseModel
                {
                    Configurations = configurationList,
                    TotalCount = configurationList.Count
                };

                return await SuccessAsync(response, "Configurations retrieved successfully");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting configurations by module: {module}",
                    ex,
                    source: "ConfigurationController.GetByModule",
                    module: "Configuration",
                    action: "GetByModule",
                    details: new { Module = module }
                );
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Reset configuration to default value
        /// </summary>
        /// <param name="id">The ID of the configuration to reset to default value</param>
        /// <returns>The response message</returns>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpPost("reset/{id}")]
        public async Task<ActionResult<GenericResponseModel>> ResetToDefault(long id)
        {
            StartProcessing();

            try
            {
                var success = await _configurationService.ResetToDefaultAsync(id, User.GetUsername() ?? "system");
                if (!success)
                    return Error<GenericResponseModel>("Failed to reset configuration to default", "RESET_FAILED");

                return await SuccessAsync(new GenericResponseModel { Data = new { Message = "Configuration reset to default successfully" } });
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to reset configuration to default");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Export configurations to JSON
        /// </summary>
        /// <param name="module">The module of the configurations to export</param>
        /// <param name="includeSystemConfigs">Whether to include system configurations</param>
        /// <returns>The JSON data of the configurations</returns>
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("export")]
        public async Task<ActionResult<GenericResponseModel>> ExportConfigurations([FromQuery] string? module = null, [FromQuery] bool includeSystemConfigs = false)
        {
            StartProcessing();

            try
            {
                var json = await _configurationService.ExportToJsonAsync(module, includeSystemConfigs);
                var response = new GenericResponseModel
                {
                    Data = json
                };
                return await SuccessAsync(response, "Configurations exported successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to export configurations");
            }
            finally
            {
                StopProcessing();
            }
        }

        /// <summary>
        /// Import configurations từ JSON
        /// </summary>
        [HttpPost("import")]
        public async Task<ActionResult<GenericResponseModel>> ImportConfigurations([FromBody] ImportConfigurationsRequest request)
        {
            StartProcessing();

            try
            {
                var importedCount = await _configurationService.ImportFromJsonAsync(
                    request.JsonData,
                    User.Identity?.Name ?? "api",
                    request.Module ?? "Core"
                );

                return await SuccessAsync(new GenericResponseModel { Data = new { ImportedCount = importedCount } }, "Configurations imported successfully");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to import configurations");
            }
            finally
            {
                StopProcessing();
            }
        }
    }
}
