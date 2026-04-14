using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Web.AspNetCore.Models
{
    
    /// <summary>
    /// ID response model
    /// </summary>
    public class IdResponseModel : BaseResponseModel
    {
        /// <summary>
        /// ID value
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Additional data
        /// </summary>
        public object? Data { get; set; }
    }
}
