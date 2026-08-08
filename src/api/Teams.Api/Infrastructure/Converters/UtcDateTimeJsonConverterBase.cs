using System.Text.Json.Serialization;

namespace Teams.Api.Infrastructure.Converters;

public abstract class UtcDateTimeJsonConverterBase<T> : JsonConverter<T>
{
    public static DateTime EnsureUtcDateTime(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc) // no offset supplied — treat as already UTC
    };
}
