using Teams.Common.Providers.Temporal;

namespace Teams.Common.UnitTests.Providers.Temporal;

public class DateTimeOffsetProviderTests
{
    [Fact]
    public void Now_ReturnsCurrentTime_WhenNoContextConfigured()
        => Assert.True((DateTimeOffsetProvider.Now - DateTimeOffset.UtcNow).TotalMilliseconds <= 3);

    [Fact]
    public void Now_ReturnsConfiguredTime_WhenContextConfigured()
    {
        var fixedTimestamp = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));
        using var ambientContext = new DateTimeOffsetProviderContext(fixedTimestamp);
        Assert.Equal(fixedTimestamp, DateTimeOffsetProvider.Now);
    }

    [Fact]
    public void Now_OnlyAppliesToScope_WhenContextConfiguredInUsingContext()
    {
        var fixedTimestamp = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));

        using (var _ = new DateTimeOffsetProviderContext(fixedTimestamp))
        {
            Assert.Equal(fixedTimestamp, DateTimeOffsetProvider.Now);
        }

        Assert.True((DateTimeOffsetProvider.Now - DateTimeOffset.UtcNow).TotalMilliseconds <= 3);
    }
}