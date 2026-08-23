using Teams.Authoriser.Auth;

namespace Teams.Authoriser.UnitTests.Auth;

public class CacheExpiryCalculatorTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Calculate_returns_the_remaining_token_lifetime_when_it_is_shorter_than_15_minutes()
    {
        var expiresAt = Now.AddMinutes(5);

        var result = CacheExpiryCalculator.Calculate(expiresAt, Now);

        Assert.Equal(TimeSpan.FromMinutes(5), result);
    }

    [Fact]
    public void Calculate_caps_at_15_minutes_when_the_token_has_longer_left()
    {
        var expiresAt = Now.AddHours(1);

        var result = CacheExpiryCalculator.Calculate(expiresAt, Now);

        Assert.Equal(TimeSpan.FromMinutes(15), result);
    }

    [Fact]
    public void Calculate_returns_exactly_15_minutes_when_the_token_has_exactly_that_long_left()
    {
        var expiresAt = Now.AddMinutes(15);

        var result = CacheExpiryCalculator.Calculate(expiresAt, Now);

        Assert.Equal(TimeSpan.FromMinutes(15), result);
    }
}