using Teams.Common.Services;

namespace Teams.Common.UnitTests.Services;

public class EnvironmentVariableAccessorTests
{
    private const string Variable = "test-environment-variable";
    private const string InitialValue = "initial-value";

    public EnvironmentVariableAccessorTests()
        => Environment.SetEnvironmentVariable(Variable, InitialValue);

    [Fact]
    public void Get_ReturnsValue_WhenValueAvailable()
    {
        var sut = CreateSut();
        var actual = sut.Get(Variable);
        Assert.Equal(InitialValue, actual);
    }

    [Fact]
    public void Get_ReturnsNull_WhenValueUnavailable()
    {
        Environment.SetEnvironmentVariable(Variable, null);
        var sut = CreateSut();
        var actual = sut.Get(Variable);
        Assert.Null(actual);
    }

    [Fact]
    public void Set_UpdatesValue_WhenValueProvided()
    {
        const string newValue = "new-value";
        var sut = CreateSut();
        sut.Set(Variable, newValue);

        var actual = Environment.GetEnvironmentVariable(Variable);
        Assert.Equal(newValue, actual);
    }

    [Fact]
    public void Set_RemovesVariable_WhenValueNull()
    {
        var sut = CreateSut();
        sut.Set(Variable, null);

        var actual = Environment.GetEnvironmentVariable(Variable);
        Assert.Null(actual);
    }

    /*
     * Private methods
     */

    private static EnvironmentVariableAccessor CreateSut() => new();
}