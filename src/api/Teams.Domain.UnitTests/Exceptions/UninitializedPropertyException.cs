using Teams.Domain.Exceptions;

namespace Teams.Domain.UnitTests.Exceptions;

public static class UninitializedPropertyExceptionTests
{
    public class Constructor
    {
        [Fact]
        public void ShouldInitializeException_WithNoParameters()
        {
            var sut = new UninitializedPropertyException();
            Assert.NotNull(sut);
        }

        [Fact]
        public void ShouldInitializeException_WithMessage()
        {
            const string message = nameof(ShouldInitializeException_WithMessage);
            var sut = new UninitializedPropertyException(message);
            Assert.Equal(message, sut.Message);
        }

        [Fact]
        public void ShouldInitializeException_WithMessage_AndInnerException()
        {
            const string message = nameof(ShouldInitializeException_WithMessage_AndInnerException);
            var innerException = new ArgumentNullException(nameof(ShouldInitializeException_WithMessage_AndInnerException), "message");
            var sut = new UninitializedPropertyException(message, innerException);
            Assert.Equal(message, sut.Message);
            Assert.Equivalent(innerException, sut.InnerException);
        }
    }

    public class For
    {
        [Fact]
        public void ShouldCreateException_WithExpectedMessage()
        {
            const string propertyName = "test-property-name";
            const string expected = $"Uninitialized property: {propertyName}";

            var actual = UninitializedPropertyException.For(propertyName);
            Assert.Equal(expected, actual.Message);
        }
    }
}