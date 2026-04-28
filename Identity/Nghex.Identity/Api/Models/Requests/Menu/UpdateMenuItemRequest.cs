using System.ComponentModel.DataAnnotations;

namespace Nghex.Identity.Api.Models.Requests
{
    /// <summary>
    /// Request model for updating a menu item
    /// </summary>
    public class UpdateMenuItemRequest : CreateMenuItemRequest
    {
        /// <summary>
        /// Menu ID
        /// </summary>
        [Required(ErrorMessage = "Menu ID is required")]
        public long Id { get; set; }

        public string UpdatedBy { get; set; } = string.Empty;
    }
}
