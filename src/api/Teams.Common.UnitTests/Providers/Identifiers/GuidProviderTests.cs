using Teams.Common.Providers.Identifiers;

namespace Teams.Common.UnitTests.Providers.Identifiers;

public class GuidProviderTests
{
    [Fact]
    public void Now_ReturnsNewGuid_WhenNoContextConfigured()
        => Assert.NotEqual(Guid.Empty, GuidProvider.New);

    [Fact]
    public void Now_ReturnsConfiguredGuid_WhenContextConfigured()
    {
        var fixedGuid = new Guid("7e0c1088-c1ef-4eff-8711-5638e9d99f4f");
        using var ambientContext = new GuidProviderContext(fixedGuid);
        Assert.Equal(fixedGuid, GuidProvider.New);
    }

    [Fact]
    public void Now_OnlyAppliesToScope_WhenContextConfiguredInUsingContext()
    {
        var fixedGuid = new Guid("2d4efedc-dfa7-49fa-8d96-00e486059f71");
        using (var _ = new GuidProviderContext(fixedGuid))
        {
            Assert.Equal(fixedGuid, GuidProvider.New);
        }
        Assert.NotEqual(fixedGuid, GuidProvider.New);
    }
}