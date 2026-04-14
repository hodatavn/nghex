namespace Nghex.Utilities
{

    /// <summary>
    /// Reflection Service class for reflection operations
    /// </summary>
    public static class ReflectionService
    {
        /// <summary>
        /// Get property value by name
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value</returns>
        public static object? GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj);
        }

        /// <summary>
        /// Set property value by name
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">Value to set</param>
        public static void SetPropertyValue(object obj, string propertyName, object? value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            property?.SetValue(obj, value);
        }

        /// <summary>
        /// Check if property exists
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>True if property exists</returns>
        public static bool HasProperty(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        /// <summary>
        /// Get all property names
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>List of property names</returns>
        public static IEnumerable<string> GetPropertyNames(object obj)
        {
            return obj.GetType().GetProperties().Select(p => p.Name);
        }

        /// <summary>
        /// Copy properties from source to target
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="excludeProperties">Properties to exclude</param>
        public static void CopyProperties(object source, object target, params string[] excludeProperties)
        {
            var sourceType = source.GetType();
            var targetType = target.GetType();

            foreach (var sourceProperty in sourceType.GetProperties())
            {
                if (excludeProperties.Contains(sourceProperty.Name))
                    continue;

                var targetProperty = targetType.GetProperty(sourceProperty.Name);
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    var value = sourceProperty.GetValue(source);
                    targetProperty.SetValue(target, value);
                }
            }
        }
    }

}