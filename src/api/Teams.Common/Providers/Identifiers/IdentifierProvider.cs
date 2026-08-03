namespace Teams.Common.Providers.Identifiers;

/// <summary>Identifier provider allowing fixture in test classes.</summary>
public static class IdentifierProvider
{
    /// <summary>Create a new identifier.</summary>
    public static string Generate
        => IdentifierProviderContext.Current?.Value ?? GuidProvider.New.ToString("N");
}