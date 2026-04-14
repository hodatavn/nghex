using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Account
{
    /// <summary>
    /// Request model for creating an account
    /// </summary>
    public class CreateAccountRequestModel : BaseRequestModel
    {
        /// <summary>
        /// Username
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Email
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        [StringLength(200, ErrorMessage = "Display name cannot exceed 200 characters")]
        public string? DisplayName { get; set; }
        
        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Additional data
        /// </summary>
        public Dictionary<string, object>? AdditionalData { get; set; }

        public override bool IsValid()
        {
            return base.IsValid() && 
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Email.Length <= 255 &&
                   Password.Length <= 100;
        }

        /// <summary>
        /// Get validation errors
        /// </summary>
        /// <returns>List of validation errors</returns>
        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();
            // Validate email
            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("Email is required");
            else if (Email.Length > 255)
                errors.Add("Email cannot exceed 255 characters");
            else if (!IsValidEmail(Email))
                errors.Add("Invalid email format");
            // Validate password
            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Password is required");
            else if (Password.Length > 100)
                errors.Add("Password cannot exceed 100 characters");
            // Validate display name
            if (!string.IsNullOrWhiteSpace(DisplayName) && DisplayName.Length > 200)
                errors.Add("Display name cannot exceed 200 characters");
            // Validate is active
            if (IsActive != true && IsActive != false)
                errors.Add("Is active must be true or false");
            // Validate additional data
            return errors;
        }

        /// <summary>
        /// Check if email is valid
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>True if valid, false otherwise</returns>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

}
