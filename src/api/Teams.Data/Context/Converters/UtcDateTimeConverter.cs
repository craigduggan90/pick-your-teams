using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Teams.Data.Context.Converters;

/// <summary>Defines conversions to manage conversion DateTime values for storage.</summary>
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    convertToProviderExpression: value => ToProvider(value),
    convertFromProviderExpression: value => FromProvider(value))
{
    private static DateTime ToProvider(DateTime value) => value.ToUniversalTime();
    private static DateTime FromProvider(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}