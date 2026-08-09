namespace Teams.Common.Providers.Temporal;

/// <summary>DateTime provider allowing fixture in test classes.</summary>
public static class DateTimeOffsetProvider
{
    /// <inheritdoc cref="DateTimeOffset.Now"/>
    public static DateTimeOffset Now
        => DateTimeOffsetProviderContext.Current?.Timestamp ?? DateTimeOffset.Now;
}