namespace Nghex.Identity.Api.Models.Requests
{
    /// <summary>
    /// Refresh Token Request
    /// </summary>
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
