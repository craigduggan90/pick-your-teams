using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Games.ResponseModels;

public record GameOrganiserModel(string Id, string Tag, string DisplayName)
{
    [ExcludeFromCodeCoverage]
    public static GameOrganiserModel Example => new(
        Id: "a694bc382d854d8385e79b2fce684090",
        Tag: "little-bobby-tables",
        DisplayName: "Robert D. Tables");
}
