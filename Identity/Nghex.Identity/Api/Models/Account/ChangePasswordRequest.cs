using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Account
{
    public class ChangePasswordRequest : BaseRequestModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Current password is required")]
        [StringLength(50, ErrorMessage = "Current password cannot exceed 50 characters")]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "New password cannot exceed 50 characters")]
        public string NewPassword { get; set; } = string.Empty;
       
        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(CurrentPassword) &&
                   !string.IsNullOrWhiteSpace(NewPassword) &&
                   Username.Length <= 50 &&
                   CurrentPassword.Length <= 50 &&
                   NewPassword.Length <= 50;
        }
        
    }
}