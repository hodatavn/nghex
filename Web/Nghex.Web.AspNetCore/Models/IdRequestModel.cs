using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Models
{
    /// <summary>
    /// ID request model
    /// </summary>
    public class IdRequestModel : BaseRequestModel
    {
        /// <summary>
        /// ID value
        /// </summary>
        [Required(ErrorMessage = "ID is required")]
        public long Id { get; set; }

        /// <summary>
        /// Validate request model
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return base.IsValid() && Id > 0;
        }

        /// <summary>
        /// Get validation errors
        /// </summary>
        /// <returns>List of validation errors</returns>
        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();

            if (Id <= 0)
                errors.Add("ID must be greater than 0");

            return errors;
        }
    }
}
