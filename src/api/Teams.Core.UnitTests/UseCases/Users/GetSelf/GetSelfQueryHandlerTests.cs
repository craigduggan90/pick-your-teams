using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Users.GetSelf;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Users.GetSelf;

public static class GetSelfQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetSelfQuery>
    {
        private GetSelfQueryHandler CreateSut() => new(UsersRepository, InvitationsRepository, ActorAccessor);

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenActorsUserDoesNotExist()
        {
            ActorAccessor.Current.Returns(new Actor("missing-user", "tag", "display-name"));
            UsersRepository.GetByIdAsync("missing-user", Arg.Any<CancellationToken>()).Returns((User?)null);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(new GetSelfQuery(), TestContext.Current.CancellationToken));

            Assert.Equal(nameof(User), exception.ResourceType);
            Assert.Equal("missing-user", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldReturnActorsUser_WhenUserExists()
        {
            var existingUser = new User("display-name", "external-id", "user@example.com", null);
            ActorAccessor.Current.Returns(new Actor(existingUser.Id, existingUser.Tag, existingUser.DisplayName));
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            var sut = CreateSut();

            var result = await sut.HandleAsync(new GetSelfQuery(), TestContext.Current.CancellationToken);

            Assert.Same(existingUser, result.User);
        }

        [Fact]
        public async Task ShouldQueryByActorsId_NotAnyOtherUser()
        {
            var existingUser = new User("display-name", "external-id", "user@example.com", null);
            ActorAccessor.Current.Returns(new Actor(existingUser.Id, existingUser.Tag, existingUser.DisplayName));
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            var sut = CreateSut();

            await sut.HandleAsync(new GetSelfQuery(), TestContext.Current.CancellationToken);

            await UsersRepository.Received(1).GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldReturnPendingInvitationsCount_ForActorsOpenInvitations()
        {
            var existingUser = new User("display-name", "external-id", "user@example.com", null);
            ActorAccessor.Current.Returns(new Actor(existingUser.Id, existingUser.Tag, existingUser.DisplayName));
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            InvitationsRepository.CountInvitationsAsync(existingUser.Id, InvitationStatusEnum.Open, Arg.Any<CancellationToken>())
                .Returns(3);
            var sut = CreateSut();

            var result = await sut.HandleAsync(new GetSelfQuery(), TestContext.Current.CancellationToken);

            Assert.Equal(3, result.PendingInvitations);
        }
    }
}
