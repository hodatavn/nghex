using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Configuration.Api.Models
{
    /// <summary>
    /// Request model cho CreateConfiguration
    /// </summary>
    public class CreateConfigurationRequest : BaseRequestModel
    {
        /// <summary>
        /// Configuration Key
        /// </summary>
        [Required(ErrorMessage = "Key is required")]
        [StringLength(200, ErrorMessage = "Key cannot exceed 200 characters")]
        public string Key { get; set; } = string.Empty;
        /// <summary>
        /// Configuration Value
        /// </summary>
        [StringLength(4000, ErrorMessage = "Value cannot exceed 4000 characters")]
        public string? Value { get; set; }
        /// <summary>
        /// Configuration Description
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
        /// <summary>
        /// Configuration Module
        /// </summary>
        [StringLength(100, ErrorMessage = "Module cannot exceed 100 characters")]
        public string? Module { get; set; }
        /// <summary>
        /// Configuration Data Type
        /// </summary>
        [StringLength(50, ErrorMessage = "DataType cannot exceed 50 characters")]
        public string DataType { get; set; } = "string";
        /// <summary>
        /// Configuration Default Value
        /// </summary>
        [StringLength(4000, ErrorMessage = "Default Value cannot exceed 4000 characters")]
        public string? DefaultValue { get; set; }
        /// <summary>
        /// Configuration Is System Config
        /// </summary>
        public bool IsSystemConfig { get; set; } = false;
        /// <summary>
        /// Configuration Is Editable
        /// </summary>
        public bool IsEditable { get; set; } = true;
        /// <summary>
        /// Configuration Is Active
        /// </summary>
        public bool IsActive { get; set; } = true;

        public override bool IsValid()
        {
            return base.IsValid() && 
            !string.IsNullOrWhiteSpace(Key) && Key.Length <= 200 &&
            (Value == null || Value.Length <= 4000) &&
            (Description == null || Description.Length <= 1000) &&
            (Module == null || Module.Length <= 100) &&
            DataType.Length <= 50 &&
            (DefaultValue == null || DefaultValue.Length <= 4000);
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors().ToList();
            
            if (string.IsNullOrWhiteSpace(Key))
                errors.Add("Key is required");
            else if (Key.Length > 200)
                errors.Add("Key cannot exceed 200 characters");

            if (!string.IsNullOrWhiteSpace(Value) && Value.Length > 4000)
                errors.Add("Value cannot exceed 4000 characters");

            if (!string.IsNullOrWhiteSpace(Description) && Description.Length > 1000)
                errors.Add("Description cannot exceed 1000 characters");

            if (!string.IsNullOrWhiteSpace(Module) && Module.Length > 100)
                errors.Add("Module cannot exceed 100 characters");
            
            return errors;
        }
    }


}