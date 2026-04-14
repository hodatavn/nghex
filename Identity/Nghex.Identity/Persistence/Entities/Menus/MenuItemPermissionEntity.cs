using Nghex.Base.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Identity.Persistence.Entities
{
    /// <summary>
    /// Persistence model for MenuItemPermission (menu-permission mapping)
    /// </summary>
    [Table("SYS_MENU_ITEM_PERMISSIONS")]
    public class MenuItemPermissionEntity : BaseEntity
    {
        /// <summary>
        /// Menu key this mapping belongs to
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("MENU_KEY")]
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>
        /// Required permission code (e.g. ACCOUNT_VIEW)
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("PERMISSION_CODE")]
        public string PermissionCode { get; set; } = string.Empty;
    }
}
