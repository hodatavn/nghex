namespace Nghex.Identity.Api.Models.Account
{

    /// <summary>
    /// Refresh Token Request Model
    /// </summary>
    public class RefreshTokenRequestModel
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}