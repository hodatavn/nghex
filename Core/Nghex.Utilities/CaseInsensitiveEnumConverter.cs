using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nghex.Utilities
{
    /// <summary>
    /// Case-insensitive JSON enum converter
    /// Allows enum values to be deserialized regardless of case
    /// </summary>
    public class CaseInsensitiveEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                    return default;

                // Try case-insensitive enum parsing
                if (Enum.TryParse<TEnum>(stringValue, ignoreCase: true, out var result))
                    return result;

                // If parsing fails, try to find by name ignoring case
                var enumNames = Enum.GetNames<TEnum>();
                var matchingName = enumNames.FirstOrDefault(name => 
                    string.Equals(name, stringValue, StringComparison.OrdinalIgnoreCase));
                
                if (matchingName != null && Enum.TryParse<TEnum>(matchingName, out result))
                    return result;

                throw new JsonException($"Unable to convert \"{stringValue}\" to {typeof(TEnum)}");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                // Allow numeric values as well
                var numericValue = reader.GetInt32();
                if (Enum.IsDefined(typeof(TEnum), numericValue))
                    return (TEnum)Enum.ToObject(typeof(TEnum), numericValue);
                
                throw new JsonException($"Unable to convert numeric value {numericValue} to {typeof(TEnum)}");
            }

            throw new JsonException($"Unable to convert token type {reader.TokenType} to {typeof(TEnum)}");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// Factory for creating case-insensitive enum converters
    /// </summary>
    public class CaseInsensitiveEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(CaseInsensitiveEnumConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }
}
