using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Models
{
    /// <summary>
    /// Flat row used for building a filtered menu tree.
    /// IsAccessible marks whether the user can directly access the node (leaf/action) versus
    /// nodes included only because they are ancestors/group headers.
    /// </summary>
    public class MenuItemAccess
    {
        public MenuItemEntity Menu { get; set; } = new();
        public bool IsAccessible { get; set; }
    }
}






