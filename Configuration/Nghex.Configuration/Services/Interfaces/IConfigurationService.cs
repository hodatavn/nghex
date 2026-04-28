using Nghex.Configuration.Api.Models;
using Nghex.Core.Configuration;

namespace Nghex.Configuration.Services.Interfaces
{
    public interface IConfigurationService : IAppConfigurationReader
    {
        IEnumerable<string> GetDataTypes();

        Task<ConfigurationResponseModel?> GetByIdAsync(long id);
        Task<ConfigurationResponseModel?> GetByKeyAsync(string key);
        Task<bool> KeyExistsAsync(string key);
        Task<IEnumerable<ConfigurationResponseModel>> GetAllAsync(bool isActive);
        Task<ConfigurationResponseModel> CreateAsync(CreateConfigurationRequest request);
        Task<bool> UpdateAsync(UpdateConfigurationRequest request);
        Task<IEnumerable<ConfigurationResponseModel>> GetByModuleAsync(string module);
        Task<bool> ResetToDefaultAsync(long id, string updatedBy);
        Task<int> ImportFromJsonAsync(string jsonData, string createdBy, string module = "Core");
        Task<string> ExportToJsonAsync(string? module = null, bool includeSystemConfigs = false);
    }
}
