using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Models
{
    /// <summary>
    /// Pagination response model
    /// </summary>
    public class PaginationResponseModel : BaseResponseModel
    {
        /// <summary>
        /// Response data
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Total count
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// Page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Has next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Has previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }
}
