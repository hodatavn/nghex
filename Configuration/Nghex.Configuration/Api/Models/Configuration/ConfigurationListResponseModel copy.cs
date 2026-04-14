using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Configuration.Api.Models
{
    /// <summary>
    /// Response model for list of roles
    /// </summary>
    public class ConfigurationListResponseModel : BaseResponseModel
    {
        /// <summary>
        /// List of roles
        /// </summary>
        public List<ConfigurationResponseModel> Configurations { get; set; } = [];

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}
