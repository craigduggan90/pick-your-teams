using Teams.Core.Exceptions;
using Teams.Core.UseCases.Users.GetUserByExternalId;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Users.GetUserByExternalId;

public static class GetUserByExternalIdQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetUserByExternalIdQuery>
    {
        private GetUserByExternalIdQueryHandler CreateSut() => new(UsersRepository);

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            UsersRepository.GetByExternalIdAsync("missing-external-id", Arg.Any<CancellationToken>()).Returns((User?)null);
            var query = new GetUserByExternalIdQuery("missing-external-id");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(query, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(User), exception.ResourceType);
            Assert.Equal("missing-external-id", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldReturnUser_WhenUserExists()
        {
            var existingUser = new User("display-name", "external-id", "user@example.com", null);
            UsersRepository.GetByExternalIdAsync(existingUser.ExternalId!, Arg.Any<CancellationToken>()).Returns(existingUser);
            var query = new GetUserByExternalIdQuery(existingUser.ExternalId!);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Same(existingUser, result);
        }
    }
}