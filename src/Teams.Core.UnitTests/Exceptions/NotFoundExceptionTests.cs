using Teams.Core.Exceptions;

namespace Teams.Core.UnitTests.Exceptions;

public static class NotFoundExceptionTests
{
    private const string ExpectedMessage = "The requested resource was not found.";

    public class Constructor
    {
        [Fact]
        public void SetsMessage_WhenCalledWithNoArguments()
        {
            var exception = new NotFoundException();

            Assert.Equal(ExpectedMessage, exception.Message);
        }

        [Fact]
        public void SetsResourceTypeAndIdentifierToNull_WhenCalledWithNoArguments()
        {
            var exception = new NotFoundException();

            Assert.Null(exception.ResourceType);
            Assert.Null(exception.ResourceIdentifier);
        }

        [Fact]
        public void SetsMessage_WhenCalledWithMemberInfo()
        {
            var exception = new NotFoundException(typeof(NotFoundException));

            Assert.Equal(ExpectedMessage, exception.Message);
        }

        [Fact]
        public void SetsResourceTypeFromMemberInfoName_WhenCalledWithMemberInfo()
        {
            var exception = new NotFoundException(typeof(NotFoundException));

            Assert.Equal(nameof(NotFoundException), exception.ResourceType);
        }

        [Fact]
        public void SetsResourceIdentifierToNull_WhenCalledWithMemberInfoOnly()
        {
            var exception = new NotFoundException(typeof(NotFoundException));

            Assert.Null(exception.ResourceIdentifier);
        }

        [Fact]
        public void SetsMessage_WhenCalledWithStringType()
        {
            var exception = new NotFoundException("CustomResource");

            Assert.Equal(ExpectedMessage, exception.Message);
        }

        [Fact]
        public void SetsResourceType_WhenCalledWithStringType()
        {
            var exception = new NotFoundException("CustomResource");

            Assert.Equal("CustomResource", exception.ResourceType);
        }

        [Fact]
        public void SetsResourceIdentifierToNull_WhenCalledWithStringTypeOnly()
        {
            var exception = new NotFoundException("CustomResource");

            Assert.Null(exception.ResourceIdentifier);
        }

        [Fact]
        public void SetsMessage_WhenCalledWithMemberInfoAndIdentifier()
        {
            var exception = new NotFoundException(typeof(NotFoundException), "abc-123");

            Assert.Equal(ExpectedMessage, exception.Message);
        }

        [Fact]
        public void SetsResourceTypeFromMemberInfoName_WhenCalledWithMemberInfoAndIdentifier()
        {
            var exception = new NotFoundException(typeof(NotFoundException), "abc-123");

            Assert.Equal(nameof(NotFoundException), exception.ResourceType);
        }

        [Fact]
        public void SetsResourceIdentifier_WhenCalledWithMemberInfoAndIdentifier()
        {
            var exception = new NotFoundException(typeof(NotFoundException), "abc-123");

            Assert.Equal("abc-123", exception.ResourceIdentifier);
        }

        [Fact]
        public void SetsResourceIdentifierUsingToString_WhenIdentifierIsNotAString()
        {
            var exception = new NotFoundException(typeof(NotFoundException), 42);

            Assert.Equal("42", exception.ResourceIdentifier);
        }

        [Fact]
        public void SetsMessage_WhenCalledWithStringTypeAndIdentifier()
        {
            var exception = new NotFoundException("CustomResource", "abc-123");

            Assert.Equal(ExpectedMessage, exception.Message);
        }

        [Fact]
        public void SetsResourceType_WhenCalledWithStringTypeAndIdentifier()
        {
            var exception = new NotFoundException("CustomResource", "abc-123");

            Assert.Equal("CustomResource", exception.ResourceType);
        }

        [Fact]
        public void SetsResourceIdentifier_WhenCalledWithStringTypeAndIdentifier()
        {
            var exception = new NotFoundException("CustomResource", "abc-123");

            Assert.Equal("abc-123", exception.ResourceIdentifier);
        }
    }
}