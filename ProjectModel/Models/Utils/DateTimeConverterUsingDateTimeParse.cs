using Models;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Use Irish culture for parsing
        var culture = new CultureInfo("en-IE");
        return DateTime.Parse(reader.GetString()!, culture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("dd-MM-yyyy"));
    }

}

public class YesNoNullEnumConverter : JsonConverter<NA_Yes_No>
{
    public override NA_Yes_No Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return NA_Yes_No.NA;
        }

        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int intValue))
        {
            if (Enum.IsDefined(typeof(NA_Yes_No), intValue))
            {
                return (NA_Yes_No)intValue;
            }
        }

        throw new JsonException($"Invalid value for {nameof(NA_Yes_No)} enum.");
    }

    public override void Write(Utf8JsonWriter writer, NA_Yes_No value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}


