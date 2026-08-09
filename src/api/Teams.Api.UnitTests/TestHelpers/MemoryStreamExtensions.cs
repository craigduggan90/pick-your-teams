using Teams.Common.Extensions;

namespace Teams.Api.UnitTests.TestHelpers;

public static class MemoryStreamExtensions
{
    public static async Task<T?> RewindAndReadAsync<T>(this MemoryStream stream)
    {
        stream.Position = 0;
        var content = await new StreamReader(stream).ReadToEndAsync();
        return content.Deserialize<T>();
    }
}