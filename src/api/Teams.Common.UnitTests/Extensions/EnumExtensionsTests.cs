using System.ComponentModel;
using Teams.Common.Extensions;

namespace Teams.Common.UnitTests.Extensions;

public static class EnumExtensionsTests
{
    public class GetName
    {
        [Fact]
        public void GetName_ReturnsEnumName()
        {
            const TestEnum value = TestEnum.Option2;
            const string expected = "Option2";
            Assert.Equal(expected, value.GetName());
        }
    }
}

internal enum TestEnum
{
    Option1,

    [Description("Option2 Description")]
    Option2
}