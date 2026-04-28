using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for account list
    /// </summary>
    public class AccountListResponse : BaseResponseModel
    {
        /// <summary>
        /// List of accounts
        /// </summary>
        public List<AccountResponse> Accounts { get; set; } = [];

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}
