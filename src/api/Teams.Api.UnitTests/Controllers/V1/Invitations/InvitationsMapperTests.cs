using Teams.Api.Controllers.V1.Invitations;
using Teams.Api.Controllers.V1.Invitations.RequestModels;
using Teams.Common.Pagination;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.UnitTests.Controllers.V1.Invitations;

public static class InvitationsMapperTests
{
    private static User GetOrganiser() => new("Test Organiser", "external-id", "organiser@test.net", null);

    private static User GetInvitee() => new("Test Invitee", "external-id-2", "invitee@test.net", null);

    private static Game GetGame(User organiser) =>
        new(organiser.Id, "Oak Leaf Leisure Centre", new DateTime(2026, 8, 10, 19, 0, 0, DateTimeKind.Utc), 60, 5)
        {
            Organiser = organiser
        };

    private static Game GetGameWithoutOrganiser() =>
        new("missing-organiser-id", "Oak Leaf Leisure Centre", new DateTime(2026, 8, 10, 19, 0, 0, DateTimeKind.Utc), 60, 5);

    // The User navigation property is only populated by EF Core's .Include (see
    // ReadOnlyInvitationsRepository) - it must be set explicitly here since these entities are
    // built in-memory, not loaded from the database.
    private static Invitation GetInvitation(Game game, User? invitee = null) =>
        new(game.Id, invitee?.Id ?? "user-id", invitee?.EmailAddress ?? "invitee@test.net") { Game = game, User = invitee };

    public class ToModel
    {
        [Fact]
        public void MapsInvitationGameOrganiserInviteeAndCreated_WhenCalled()
        {
            var organiser = GetOrganiser();
            var invitee = GetInvitee();
            var game = GetGame(organiser);
            var invitation = GetInvitation(game, invitee);
            invitation.Accept();

            var result = invitation.ToModel();

            Assert.Equal(invitation.Id, result.Id);
            Assert.Equal(nameof(InvitationStatusEnum.Accepted), result.Status);

            Assert.Equal(game.Id, result.Game.Id);
            Assert.Equal(game.StartTime, result.Game.StartTime);
            Assert.Equal(game.Duration, result.Game.Duration);
            Assert.Equal(game.Location, result.Game.Location);

            Assert.Equal(organiser.Id, result.Organiser!.Id);
            Assert.Equal(organiser.Tag, result.Organiser.Tag);
            Assert.Equal(organiser.DisplayName, result.Organiser.DisplayName);

            Assert.Equal(invitee.Id, result.Invitee!.Id);
            Assert.Equal(invitee.Tag, result.Invitee.Tag);
            Assert.Equal(invitee.DisplayName, result.Invitee.DisplayName);

            Assert.Equal(invitation.DateCreated, result.Created);
        }

        [Fact]
        public void SetsOrganiserToNull_WhenGameHasNoOrganiser()
        {
            var game = GetGameWithoutOrganiser();
            var invitation = GetInvitation(game);

            var result = invitation.ToModel();

            Assert.Null(result.Organiser);
        }

        [Fact]
        public void SetsInviteeToNull_WhenInvitationHasNoUser()
        {
            var organiser = GetOrganiser();
            var game = GetGame(organiser);
            var invitation = GetInvitation(game);

            var result = invitation.ToModel();

            Assert.Null(result.Invitee);
        }
    }

    public class ToDetailModel
    {
        [Fact]
        public void MapsInvitationGameOrganiserInviteeAndTimestamps_WhenCalled()
        {
            var organiser = GetOrganiser();
            var invitee = GetInvitee();
            var game = GetGame(organiser);
            var invitation = GetInvitation(game, invitee);
            invitation.Decline();

            var result = invitation.ToDetailModel();

            Assert.Equal(invitation.Id, result.Id);
            Assert.Equal(nameof(InvitationStatusEnum.Declined), result.Status);

            Assert.Equal(game.Id, result.Game.Id);
            Assert.Equal(game.StartTime, result.Game.StartTime);
            Assert.Equal(game.Duration, result.Game.Duration);
            Assert.Equal(game.Location, result.Game.Location);

            Assert.Equal(organiser.Id, result.Organiser!.Id);
            Assert.Equal(organiser.Tag, result.Organiser.Tag);
            Assert.Equal(organiser.DisplayName, result.Organiser.DisplayName);

            Assert.Equal(invitee.Id, result.Invitee!.Id);
            Assert.Equal(invitee.Tag, result.Invitee.Tag);
            Assert.Equal(invitee.DisplayName, result.Invitee.DisplayName);

            Assert.Equal(invitation.DateCreated, result.Created);
            Assert.Equal(invitation.DateModified, result.Modified);
        }

        [Fact]
        public void SetsOrganiserToNull_WhenGameHasNoOrganiser()
        {
            var game = GetGameWithoutOrganiser();
            var invitation = GetInvitation(game);

            var result = invitation.ToDetailModel();

            Assert.Null(result.Organiser);
        }

        [Fact]
        public void SetsInviteeToNull_WhenInvitationHasNoUser()
        {
            var organiser = GetOrganiser();
            var game = GetGame(organiser);
            var invitation = GetInvitation(game);

            var result = invitation.ToDetailModel();

            Assert.Null(result.Invitee);
        }
    }

    public class ToCommandFromCreateInvitationsRequestModel
    {
        [Fact]
        public void MapsGameIdAndUserTags_WhenCalled()
        {
            var model = new CreateInvitationsRequestModel("game-id", ["tag-one", "tag-two"]);

            var result = model.ToCommand();

            Assert.Equal(model.GameId, result.GameId);
            Assert.Equal(model.UserTags, result.UserTags);
        }
    }

    public class ToQuery
    {
        [Fact]
        public void MapsAllSimpleProperties_WhenCalled()
        {
            var model = new GetInvitationsRequestModel(
                GameId: "game-id",
                UserId: "user-id",
                EmailAddress: "player@test.net",
                Status: nameof(InvitationStatusEnum.Accepted),
                CreatedFrom: new DateTime(2026, 1, 2),
                CreatedTo: new DateTime(2026, 1, 3),
                ModifiedFrom: new DateTime(2026, 1, 4),
                ModifiedTo: new DateTime(2026, 1, 5),
                PageSize: 25,
                Cursor: null);

            var result = model.ToQuery();

            Assert.Equal(model.GameId, result.GameId);
            Assert.Equal(model.UserId, result.UserId);
            Assert.Equal(model.EmailAddress, result.EmailAddress);
            Assert.Equal(model.CreatedFrom, result.CreatedFrom);
            Assert.Equal(model.CreatedTo, result.CreatedTo);
            Assert.Equal(model.ModifiedFrom, result.ModifiedFrom);
            Assert.Equal(model.ModifiedTo, result.ModifiedTo);
            Assert.Equal(model.PageSize, result.PageSize);
        }

        [Theory]
        [InlineData("Accepted", InvitationStatusEnum.Accepted)]
        [InlineData("declined", InvitationStatusEnum.Declined)]
        public void ParsesStatusCaseInsensitively_WhenValid(string status, InvitationStatusEnum expected)
        {
            var model = new GetInvitationsRequestModel(Status: status);

            var result = model.ToQuery();

            Assert.Equal(expected, result.Status);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("NotARealStatus")]
        public void SetsStatusToNull_WhenInvalidOrMissing(string? status)
        {
            var model = new GetInvitationsRequestModel(Status: status);

            var result = model.ToQuery();

            Assert.Null(result.Status);
        }

        [Fact]
        public void SetsCursorToNull_WhenCursorIsNull()
        {
            var model = new GetInvitationsRequestModel(Cursor: null);

            var result = model.ToQuery();

            Assert.Null(result.Cursor);
        }

        [Fact]
        public void SetsCursorToNull_WhenCursorIsInvalid()
        {
            var model = new GetInvitationsRequestModel(Cursor: "not-a-valid-cursor!!");

            var result = model.ToQuery();

            Assert.Null(result.Cursor);
        }

        [Fact]
        public void DecodesCursor_WhenCursorIsValid()
        {
            ((long?)12345).TryEncodeCursor(out var encodedCursor);
            var model = new GetInvitationsRequestModel(Cursor: encodedCursor);

            var result = model.ToQuery();

            Assert.Equal(12345, result.Cursor);
        }
    }
}