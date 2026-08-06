using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.Utilities;

/// <summary>
/// The backend sends money amounts (prices, totals) as JSON strings
/// like "12.50" instead of plain numbers, to avoid floating-point
/// rounding mistakes. This converter lets our C# "decimal" fields
/// read those strings (or plain numbers, just in case) without every
/// screen having to do that parsing by hand.
/// </summary>
public class DecimalJsonConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString();
            return decimal.Parse(text, CultureInfo.InvariantCulture);
        }
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
