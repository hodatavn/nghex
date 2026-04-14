using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.MenuItem
{
    /// <summary>
    /// Response model for list of menu items
    /// </summary>
    public class MenuItemListResponseModel : BaseResponseModel
    {
        /// <summary>
        /// List of menu items
        /// </summary>
        public List<MenuItemResponseModel> MenuItems { get; set; } = [];

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}




