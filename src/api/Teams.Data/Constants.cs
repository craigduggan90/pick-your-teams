using System.Diagnostics.CodeAnalysis;

namespace Teams.Data;

/// <summary>Constants used throughout the project.</summary>
[ExcludeFromCodeCoverage]
internal static class Constants
{
    /// <summary>The default number of items to retrieve per page of data.</summary>
    public const int DefaultPageSize = 25;
}