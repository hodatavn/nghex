using System.ComponentModel.DataAnnotations;

namespace Nghex.Identity.Api.Models.Requests;

public class ResetPasswordRequest
{
    /// <summary>
    /// Username to reset password
    /// </summary>
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
}
