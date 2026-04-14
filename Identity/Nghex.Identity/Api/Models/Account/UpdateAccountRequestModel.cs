using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Account
{
    /// <summary>
    /// Request model cho cập nhật Account
    /// </summary>
    public class UpdateAccountRequestModel : BaseRequestModel
    {
        /// <summary>
        /// Account ID
        /// </summary>
        [Required(ErrorMessage = "Account ID is required")]
        public long AccountId { get; set; }
/// <summary>
        /// Username
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Email
        /// </summary>
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }
        
        /// <summary>
        /// Display name
        /// </summary>
        [StringLength(200, ErrorMessage = "Display name cannot exceed 200 characters")]
        public string? DisplayName { get; set; }
        
        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        public override bool IsValid()
        {
            return base.IsValid() && 
                   Email!.Length <= 255;
        }
    }

}
