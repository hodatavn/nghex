using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for list of menu items
    /// </summary>
    public class MenuItemListResponse : BaseResponseModel
    {
        /// <summary>
        /// List of menu items
        /// </summary>
        public List<MenuItemResponse> MenuItems { get; set; } = [];

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}
