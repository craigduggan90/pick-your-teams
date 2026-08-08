using System.Text.Json;

namespace Teams.Api.Infrastructure.Converters;

public class UtcDateTimeJsonConverter : UtcDateTimeJsonConverterBase<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        EnsureUtcDateTime(reader.GetDateTime());

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Kind == DateTimeKind.Utc ? value : EnsureUtcDateTime(value));
}
