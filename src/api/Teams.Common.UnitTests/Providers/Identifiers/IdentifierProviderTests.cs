using Teams.Common.Providers.Identifiers;

namespace Teams.Common.UnitTests.Providers.Identifiers;

public class IdentifierProviderTests
{
    [Fact]
    public void Now_ReturnsNewIdentifier_WhenNoContextConfigured()
    {
        var guid = Guid.NewGuid();
        using var guidFixture = new GuidProviderContext(guid);
        Assert.Equal($"{guid:N}", IdentifierProvider.Generate);
    }

    [Fact]
    public void Now_ReturnsConfiguredIdentifier_WhenContextConfigured()
    {
        const string fixedIdentifier = "identifier";
        using var ambientContext = new IdentifierProviderContext(fixedIdentifier);
        Assert.Equal(fixedIdentifier, IdentifierProvider.Generate);
    }

    [Fact]
    public void Now_OnlyAppliesToScope_WhenContextConfiguredInUsingContext()
    {
        const string fixedIdentifier = "fixed identifier";
        using (var _ = new IdentifierProviderContext(fixedIdentifier))
        {
            Assert.Equal(fixedIdentifier, IdentifierProvider.Generate);
        }
        Assert.NotEqual(fixedIdentifier, IdentifierProvider.Generate);
    }
}