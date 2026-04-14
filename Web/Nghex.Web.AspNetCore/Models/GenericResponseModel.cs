using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Models
{
    /// <summary>
    /// Generic response model for simple operations
    /// </summary>
    public class GenericResponseModel : BaseResponseModel
    {
        /// <summary>
        /// Response data
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Total count if pagination is present
        /// </summary>
        public long? TotalCount { get; set; }

        /// <summary>
        /// Page number if pagination is present
        /// </summary>
        public int? PageNumber { get; set; }

        /// <summary>
        /// Page size if pagination is present
        /// </summary>
        public int? PageSize { get; set; }
    }
}

