using Teams.Core.Exceptions;
using Teams.Core.UseCases.Users.GetUserById;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Users.GetUserById;

public static class GetUserByIdQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetUserByIdQuery>
    {
        private GetUserByIdQueryHandler CreateSut() => new(UsersRepository, InvitationsRepository);

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            UsersRepository.GetByIdAsync("missing-user", Arg.Any<CancellationToken>()).Returns((User?)null);
            var query = new GetUserByIdQuery("missing-user");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(query, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(User), exception.ResourceType);
            Assert.Equal("missing-user", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldReturnUser_WhenUserExists()
        {
            var existingUser = new User("display-name", "external-id", "user@example.com", null);
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            var query = new GetUserByIdQuery(existingUser.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Same(existingUser, result.User);
        }

        [Fact]
        public async Task ShouldReturnPendingInvitationsCount_ForRequestedUsersOpenInvitations()
        {
            var existingUser = new User("display-name", "external-id", "user@example.com", null);
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            InvitationsRepository.CountInvitationsAsync(existingUser.Id, InvitationStatusEnum.Open, Arg.Any<CancellationToken>())
                .Returns(5);
            var query = new GetUserByIdQuery(existingUser.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Equal(5, result.PendingInvitations);
        }
    }
}