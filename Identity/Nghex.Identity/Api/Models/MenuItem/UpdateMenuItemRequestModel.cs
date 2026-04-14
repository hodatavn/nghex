using System.ComponentModel.DataAnnotations;

namespace Nghex.Identity.Api.Models.MenuItem
{
    /// <summary>
    /// Request model for updating a menu item
    /// </summary>
    public class UpdateMenuItemRequestModel : CreateMenuItemRequestModel
    {
        /// <summary>
        /// Menu ID
        /// </summary>
        [Required(ErrorMessage = "Menu ID is required")]
        public long Id { get; set; }
    }
}




