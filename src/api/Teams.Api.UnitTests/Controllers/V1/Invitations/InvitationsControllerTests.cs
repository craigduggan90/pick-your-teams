using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Teams.Api.Controllers.V1.Invitations;
using Teams.Api.Controllers.V1.Invitations.RequestModels;
using Teams.Api.Controllers.V1.Invitations.ResponseModels;
using Teams.Common.Pagination;
using Teams.Common.Providers.Identifiers;
using Teams.Core.CQRS;
using Teams.Core.UseCases.Invitations.AcceptInvitation;
using Teams.Core.UseCases.Invitations.CreateInvitations;
using Teams.Core.UseCases.Invitations.DeclineInvitation;
using Teams.Core.UseCases.Invitations.GetInvitationById;
using Teams.Core.UseCases.Invitations.GetInvitations;
using Teams.Domain.Entities;
using Teams.Domain.Extensions;

namespace Teams.Api.UnitTests.Controllers.V1.Invitations;

/// <summary>
/// Controllers are really tested through integration tests, these really just serve to ensure that we're injecting
/// values into mappers correctly.
/// </summary>
public static class InvitationsControllerTests
{
    public abstract class InvitationsControllerTestsBase
    {
        protected readonly IMediator Mediator = Substitute.For<IMediator>();

        private InvitationsController? _sut;

        protected InvitationsController GetOrCreateSut() =>
            _sut ??= new InvitationsController(Mediator)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

        protected static User GetOrganiser(string? id = null, string displayName = "Test Organiser")
        {
            using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
            return new User(displayName, $"external-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@test.net", null);
        }

        protected static Game GetGame(User organiser, string? id = null)
        {
            using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
            return new Game(organiser.Id, "Test Venue", DateTime.UtcNow, 60, 5) { Organiser = organiser };
        }

        protected static Invitation GetInvitation(
            Game game, string? id = null, string? userId = null, string? emailAddress = null)
        {
            using var idFix = new IdentifierProviderContext(id ?? Guid.NewGuid().ToString("N"));
            return new Invitation(game.Id, userId ?? "user-id", emailAddress ?? "invitee@test.net") { Game = game };
        }

        protected static void AssertResultValue<TResult, TValue>(IActionResult result, TValue expected)
            where TResult : ObjectResult
        {
            var objectResult = Assert.IsType<TResult>(result);
            var actual = Assert.IsType<TValue>(objectResult.Value);
            Assert.Equivalent(expected, actual);
        }
    }

    public class GetInvitations : InvitationsControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            var request = new GetInvitationsRequestModel();
            var organiser = GetOrganiser();
            var game = GetGame(organiser);
            IReadOnlyCollection<Invitation> invitations = [GetInvitation(game), GetInvitation(game)];

            Mediator.SendAsync(Arg.Any<GetInvitationsQuery>(), Arg.Any<CancellationToken>())
                .Returns(invitations);

            var expected = invitations.ToPagedList(InvitationsMapper.ToModel);

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetInvitations(request, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, PagedList<InvitationModel>>(rawResult, expected);

            await Mediator.Received(1).SendAsync(Arg.Any<GetInvitationsQuery>(), Arg.Any<CancellationToken>());
        }
    }

    public class CreateInvitations : InvitationsControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnCreated_WhenSuccess()
        {
            var requestModel = new CreateInvitationsRequestModel("game-id", ["tag-one", "tag-two"]);

            var sut = GetOrCreateSut();
            var rawResult = await sut.CreateInvitations(requestModel, TestContext.Current.CancellationToken);

            var result = Assert.IsType<StatusCodeResult>(rawResult);
            Assert.Equal(201, result.StatusCode);

            await Mediator.Received(1).SendAsync(
                Arg.Is<CreateInvitationsCommand>(c =>
                    c.GameId == requestModel.GameId && c.UserTags.SequenceEqual(requestModel.UserTags)),
                Arg.Any<CancellationToken>());
        }
    }

    public class GetInvitationById : InvitationsControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnOk_WhenSuccess()
        {
            const string id = "test-id";
            var organiser = GetOrganiser();
            var game = GetGame(organiser);
            var invitation = GetInvitation(game, id: id);

            Mediator.SendAsync(Arg.Any<GetInvitationByIdQuery>(), Arg.Any<CancellationToken>())
                .Returns(invitation);

            var expected = invitation.ToDetailModel();

            var sut = GetOrCreateSut();
            var rawResult = await sut.GetInvitationById(id, TestContext.Current.CancellationToken);

            AssertResultValue<OkObjectResult, InvitationDetailModel>(rawResult, expected);

            await Mediator.Received(1).SendAsync(Arg.Is<GetInvitationByIdQuery>(q => q.Id == id), Arg.Any<CancellationToken>());
        }
    }

    public class AcceptInvitation : InvitationsControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenSuccess()
        {
            const string id = "test-id";
            var organiser = GetOrganiser();
            var game = GetGame(organiser);
            var invitation = GetInvitation(game, id: id);

            Mediator.SendAsync(Arg.Any<AcceptInvitationCommand>(), Arg.Any<CancellationToken>())
                .Returns(invitation);

            var sut = GetOrCreateSut();
            var rawResult = await sut.AcceptInvitation(id, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(rawResult);

            await Mediator.Received(1).SendAsync(Arg.Is<AcceptInvitationCommand>(c => c.Id == id), Arg.Any<CancellationToken>());
        }
    }

    public class DeclineInvitation : InvitationsControllerTestsBase
    {
        [Fact]
        public async Task ShouldReturnNoContent_WhenSuccess()
        {
            const string id = "test-id";
            var organiser = GetOrganiser();
            var game = GetGame(organiser);
            var invitation = GetInvitation(game, id: id);

            Mediator.SendAsync(Arg.Any<DeclineInvitationCommand>(), Arg.Any<CancellationToken>())
                .Returns(invitation);

            var sut = GetOrCreateSut();
            var rawResult = await sut.DeclineInvitation(id, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(rawResult);

            await Mediator.Received(1).SendAsync(Arg.Is<DeclineInvitationCommand>(c => c.Id == id), Arg.Any<CancellationToken>());
        }
    }
}