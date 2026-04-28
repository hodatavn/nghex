using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nghex.Configuration.Api.Models;
using Nghex.Web.AspNetCore.Controllers;
using Nghex.Web.AspNetCore.Models;
using Nghex.Identity.Enum;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Configuration.Services.Interfaces;

namespace Nghex.Configuration.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationController(
        IConfigurationService configurationService,
        ILoggingService loggingService,
        IOptions<PerformanceTrackingOptions> options) : BaseController(loggingService, options)
    {
        private readonly IConfigurationService _configurationService = configurationService;

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("data-types")]
        public ActionResult GetDataTypes()
        {
            try
            {
                var dataTypes = _configurationService.GetDataTypes()
                    .Select(type => new DataTypeResponseModel { Name = type, Value = type }).ToList();
                return Ok(new DataTypeListResponseModel { DataTypes = dataTypes, Count = dataTypes.Count });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("all")]
        public async Task<ActionResult<ConfigurationListResponseModel>> GetAllConfigurations()
        {
            StartProcessing();
            try
            {
                var configurations = await _configurationService.GetAllAsync(true);
                var list = configurations.ToList();
                return await SuccessAsync(new ConfigurationListResponseModel
                {
                    Configurations = list,
                    TotalCount = list.Count
                }, "Configurations retrieved successfully");
            }
            catch (Exception ex) { return await HandleExceptionAsync<ConfigurationListResponseModel>(ex, "Failed to get configurations"); }
            finally { StopProcessing(); }
        }

        [HttpPost("create")]
        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin)]
        public async Task<ActionResult<GenericResponseModel>> CreateConfiguration([FromBody] CreateConfigurationRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<GenericResponseModel>(request.GetValidationErrors());

                request.CreatedBy = User.GetUsername() ?? "system";

                var configuration = await _configurationService.CreateAsync(request);
                return await SuccessAsync(new GenericResponseModel { Data = configuration }, "Configuration created successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to create configuration"); }
            finally { StopProcessing(); }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpPut("update")]
        public async Task<ActionResult<GenericResponseModel>> UpdateConfiguration([FromBody] UpdateConfigurationRequest request)
        {
            StartProcessing();
            try
            {
                if (!request.IsValid())
                    return ValidationError<GenericResponseModel>(request.GetValidationErrors());

                request.UpdatedBy = User.GetUsername() ?? "system";

                var success = await _configurationService.UpdateAsync(request);
                if (!success)
                    return Error<GenericResponseModel>("Failed to update configuration", "UPDATE_FAILED");

                var configuration = await _configurationService.GetByIdAsync(request.Id);
                return await SuccessAsync(new GenericResponseModel { Data = configuration }, "Configuration updated successfully");
            }
            catch (InvalidOperationException ex) { return Error<GenericResponseModel>(ex.Message, "INVALID_OPERATION"); }
            catch (ArgumentException ex) { return Error<GenericResponseModel>(ex.Message, "VALIDATION_ERROR"); }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to update configuration"); }
            finally { StopProcessing(); }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("module/{module}")]
        public async Task<ActionResult<ConfigurationListResponseModel>> GetByModule(string module)
        {
            StartProcessing();
            try
            {
                var configurations = await _configurationService.GetByModuleAsync(module);
                var list = configurations.ToList();
                return await SuccessAsync(new ConfigurationListResponseModel
                {
                    Configurations = list,
                    TotalCount = list.Count
                }, "Configurations retrieved successfully");
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
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to reset configuration to default"); }
            finally { StopProcessing(); }
        }

        [AuthorizeByRoleLevel(RoleLevel.SuperAdmin, RoleLevel.Admin)]
        [HttpGet("export")]
        public async Task<ActionResult<GenericResponseModel>> ExportConfigurations([FromQuery] string? module = null, [FromQuery] bool includeSystemConfigs = false)
        {
            StartProcessing();
            try
            {
                var json = await _configurationService.ExportToJsonAsync(module, includeSystemConfigs);
                return await SuccessAsync(new GenericResponseModel { Data = json }, "Configurations exported successfully");
            }
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to export configurations"); }
            finally { StopProcessing(); }
        }

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
            catch (Exception ex) { return await HandleExceptionAsync<GenericResponseModel>(ex, "Failed to import configurations"); }
            finally { StopProcessing(); }
        }
    }
}
