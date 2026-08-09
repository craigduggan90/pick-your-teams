using Teams.Domain.Exceptions;

namespace Teams.Domain.UnitTests.Exceptions;

public static class EntityUpdateExceptionTests
{
    public class Constructor
    {
        [Fact]
        public void ShouldInitializeException_WithNoParameters()
        {
            var sut = new EntityUpdateException();
            Assert.NotNull(sut);
        }

        [Fact]
        public void ShouldInitializeException_WithMessage()
        {
            const string message = nameof(ShouldInitializeException_WithMessage);
            var sut = new EntityUpdateException(message);
            Assert.Equal(message, sut.Message);
        }

        [Fact]
        public void ShouldInitializeException_WithMessage_AndInnerException()
        {
            const string message = nameof(ShouldInitializeException_WithMessage_AndInnerException);
            var innerException = new ArgumentNullException(nameof(ShouldInitializeException_WithMessage_AndInnerException), "message");
            var sut = new EntityUpdateException(message, innerException);
            Assert.Equal(message, sut.Message);
            Assert.Equivalent(innerException, sut.InnerException);
        }
    }

    public class ForMissingProperty
    {
        [Fact]
        public void ShouldCreateException_WithExpectedMessage()
        {
            var parentType = typeof(EntityUpdateException);
            const string propertyName = nameof(EntityUpdateException.Message);
            var expected = $"Property not found or inaccessible: '{parentType.Name}.{propertyName}'.";
            var actual = EntityUpdateException.ForMissingProperty(parentType, propertyName);
            Assert.Equal(expected, actual.Message);
        }
    }

    public class ForIncorrectType
    {
        [Fact]
        public void ShouldCreateException_WithExpectedMessage()
        {
            var parentType = typeof(EntityUpdateException);
            const string propertyName = nameof(EntityUpdateException.Message);
            var propertyType = typeof(string);
            var valueType = typeof(int);
            var expected = $"Cannot assign {valueType.Name} to {propertyType.Name} member: '{parentType.Name}.{propertyName}'.";
            var actual = EntityUpdateException.ForIncorrectType(parentType, propertyName, propertyType, valueType);
            Assert.Equal(expected, actual.Message);
        }
    }
}