using Teams.Common.Extensions;
using System.Text.Json;

namespace Teams.Common.UnitTests.Extensions;

public class JsonSerializationExtensionsTests
{
    [Fact]
    public void Deserialize_ReturnsNull_WhenInputIsNull()
    {
        string? input = null;
        var actual = input.Deserialize<object>();
        Assert.Null(actual);
    }

    [Fact]
    public void Deserialize_ReturnsObject_WhenInputDeserialized()
    {
        const string input = "{ \"property\": \"value\" }";
        var actual = input.Deserialize<ExampleObject>();
        Assert.Equivalent(new ExampleObject("value"), actual);
    }

    [Fact]
    public void Deserialize_AppliesOptions_WhenProvided()
    {
        const string input = "{ \"property\": \"value\" }";
        var actual = input.Deserialize<ExampleObject>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false
        });
        Assert.Null(actual!.Property);
    }

    [Fact]
    public void Serialize_ReturnsNull_WhenInputIsNull()
    {
        object? input = null;
        var actual = input.Serialize();
        Assert.Equal("null", actual);
    }

    [Fact]
    public void Serialize_ReturnsJson_WhenInputSerialized()
    {
        var input = new ExampleObject("Value");
        const string expected = "{\"property\":\"Value\"}";
        var actual = input.Serialize();
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Serialize_AppliesOptions_WhenProvided()
    {
        var input = new ExampleObject("Value");
        const string expected = "{\"Property\":\"Value\"}";
        var actual = input.Serialize(new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });
        Assert.Equivalent(expected, actual);
    }

    private sealed record ExampleObject(string Property);
}