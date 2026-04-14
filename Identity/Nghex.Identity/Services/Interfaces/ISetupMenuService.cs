using Nghex.Identity.Models;

namespace Nghex.Identity.Services.Interfaces
{
    public interface ISetupMenuService
    {
        Task<IReadOnlyList<MenuNodeDto>> GetSetupMenuAsync(CancellationToken cancellationToken = default);
    }
}


