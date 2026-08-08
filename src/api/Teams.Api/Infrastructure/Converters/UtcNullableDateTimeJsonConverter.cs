using System.Text.Json;

namespace Teams.Api.Infrastructure.Converters;

public class UtcNullableDateTimeConverter : UtcDateTimeJsonConverterBase<DateTime?> 
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Null ? null : EnsureUtcDateTime(reader.GetDateTime());

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        // System.Text.Json intercepts null itself for a Nullable<T> converter and never calls Write with one -
        // the ! here documents that guarantee rather than defending against it.
        writer.WriteStringValue(value!.Value.Kind == DateTimeKind.Utc
            ? value.Value
            : EnsureUtcDateTime(value.Value));
    }
}