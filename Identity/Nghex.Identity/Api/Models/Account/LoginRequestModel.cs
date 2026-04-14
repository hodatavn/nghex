
namespace Nghex.Identity.Api.Models.Account
{


    /// <summary>
    /// Request model for login
    /// </summary>
    public class LoginRequestModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }
}