using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Configuration.Api.Models
{
    public class UpdateConfigurationRequest : CreateConfigurationRequest
    {
        /// <summary>
        /// Configuration ID
        /// </summary>
        [Required(ErrorMessage = "Configuration ID is required")]
        public long Id { get; set; }
    }
}