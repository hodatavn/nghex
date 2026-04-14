using Nghex.Licensing.Models;

namespace Nghex.Licensing.Interfaces
{
    public interface ILicenseSettingsService
    {
        Task<LicenseSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(LicenseSettings settings);
    }
}