using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Models
{
    /// <summary>
    /// Generic request model for simple operations
    /// </summary>
    public class GenericRequestModel : BaseRequestModel
    {
        /// <summary>
        /// Data payload
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Additional parameters
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }
    }

}