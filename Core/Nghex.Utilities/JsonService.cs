using System.Text.Json;

namespace Nghex.Utilities
{

    /// <summary>
    /// JSON Service class for JSON operations
    /// </summary>
    public static class JsonService
    {
        /// <summary>
        /// Serialize object to JSON string
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>JSON string</returns>
        public static string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Deserialize JSON string to object
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="json">JSON string</param>
        /// <returns>Deserialized object</returns>
        public static T? FromJson<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Deserialize JSON string to object with error handling
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="json">JSON string</param>
        /// <param name="error">Error message if deserialization fails</param>
        /// <returns>Deserialized object</returns>
        public static T? FromJson<T>(string json, out string? error)
        {
            error = null;
            try
            {
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return default;
            }
        }
    }

}