using Teams.Core.Exceptions;
using Teams.Core.UseCases.Invitations.GetInvitationById;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Invitations.GetInvitationById;

public static class GetInvitationByIdQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetInvitationByIdQuery>
    {
        private GetInvitationByIdQueryHandler CreateSut() => new(InvitationsRepository);

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenInvitationDoesNotExist()
        {
            InvitationsRepository.GetByIdAsync("missing-invitation", Arg.Any<CancellationToken>()).Returns((Invitation?)null);
            var query = new GetInvitationByIdQuery("missing-invitation");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(query, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Invitation), exception.ResourceType);
            Assert.Equal("missing-invitation", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldReturnInvitation_WhenInvitationExists()
        {
            var game = new Game("organiser-id", "location", DateTime.UtcNow, 60, 5);
            var existingInvitation = new Invitation(game.Id, "user-id", "player@example.com") { Game = game };
            InvitationsRepository.GetByIdAsync(existingInvitation.Id, Arg.Any<CancellationToken>()).Returns(existingInvitation);
            var query = new GetInvitationByIdQuery(existingInvitation.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Same(existingInvitation, result);
        }
    }
}