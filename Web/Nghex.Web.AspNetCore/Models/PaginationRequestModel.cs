using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Models
{
    /// <summary>
    /// Pagination request model
    /// </summary>
    public class PaginationRequestModel : BaseRequestModel
    {
        /// <summary>
        /// Page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page size
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Page size must be between 1 and 1000")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Sort field
        /// </summary>
        [StringLength(100)]
        public string? SortField { get; set; }

        /// <summary>
        /// Sort direction (asc/desc)
        /// </summary>
        [StringLength(10)]
        public string SortDirection { get; set; } = "asc";

        /// <summary>
        /// Search term
        /// </summary>
        [StringLength(500)]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Additional filters
        /// </summary>
        public Dictionary<string, object>? Filters { get; set; }

        /// <inheritdoc/>
        public override bool IsValid()
        {
            return base.IsValid() && 
                    PageNumber > 0 && 
                    PageSize > 0 && 
                    PageSize <= 1000 &&
                    (string.IsNullOrEmpty(SortDirection) || SortDirection.ToLower() is "asc" or "desc");
        }

        // public override IEnumerable<string> GetValidationErrors()
        
        /// <summary>
        /// Get validation errors
        /// </summary>
        /// <returns>List of validation errors</returns>
        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();

            if (PageNumber <= 0)
                errors.Add("Page number must be greater than 0");

            if (PageSize <= 0)
                errors.Add("Page size must be greater than 0");

            if (PageSize > 1000)
                errors.Add("Page size cannot exceed 1000");

            if (!string.IsNullOrEmpty(SortDirection) && SortDirection.ToLower() is not "asc" and not "desc")
                errors.Add("Sort direction must be 'asc' or 'desc'");

            return errors;
        }
    }
}