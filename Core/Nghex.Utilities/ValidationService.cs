using System.ComponentModel.DataAnnotations;

namespace Nghex.Utilities
{
    /// <summary>
    /// Validation Service class for validation operations
    /// </summary>
    public static class ValidationService
    {
        /// <summary>
        /// Validate object and return validation errors
        /// </summary>
        /// <param name="obj">Object to validate</param>
        /// <returns>List of validation errors</returns>
        public static List<string> ValidateObject(object obj)
        {
            var errors = new List<string>();
            var context = new ValidationContext(obj);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(obj, context, results, true))
            {
                foreach (var result in results)
                {
                    errors.Add(result.ErrorMessage ?? "Validation error");
                }
            }

            return errors;
        }

        /// <summary>
        /// Validate property of object
        /// </summary>
        /// <param name="obj">Object to validate</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>List of validation errors</returns>
        public static List<string> ValidateProperty(object obj, string propertyName)
        {
            var errors = new List<string>();
            var property = obj.GetType().GetProperty(propertyName);
            
            if (property == null)
            {
                errors.Add($"Property '{propertyName}' not found");
                return errors;
            }

            var value = property.GetValue(obj);
            var context = new ValidationContext(obj) { MemberName = propertyName };
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateProperty(value, context, results))
            {
                foreach (var result in results)
                {
                    errors.Add(result.ErrorMessage ?? "Validation error");
                }
            }

            return errors;
        }

        /// <summary>
        /// Check if object is valid
        /// </summary>
        /// <param name="obj">Object to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValid(object obj)
        {
            var context = new ValidationContext(obj);
            return Validator.TryValidateObject(obj, context, null, true);
        }
    }

}