namespace Teams.Authoriser.Auth;

/// <summary>Caps a cache entry's lifetime at the token's own remaining lifetime, so a resolved
/// user is never cached past the point its token would fail signature validation anyway.</summary>
public static class CacheExpiryCalculator
{
    private static readonly TimeSpan MaximumTtl = TimeSpan.FromMinutes(15);

    public static TimeSpan Calculate(DateTime tokenExpiresAtUtc, DateTime nowUtc)
    {
        var remaining = tokenExpiresAtUtc - nowUtc;
        return remaining < MaximumTtl ? remaining : MaximumTtl;
    }
}