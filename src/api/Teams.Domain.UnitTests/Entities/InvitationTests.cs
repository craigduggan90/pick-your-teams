using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Domain.Exceptions;
using Teams.Domain.UnitTests.TestHelpers;

namespace Teams.Domain.UnitTests.Entities;

public static class InvitationTests
{
    public abstract class InvitationTestsBase
    {
        protected const string DefaultGameId = "game-001";
        protected const string DefaultUserId = "user-001";
        protected const string DefaultEmailAddress = "player@example.com";

        protected static Invitation CreateInvitation(string? userId = null, Action<Invitation>? setup = null)
        {
            var invitation = new Invitation(DefaultGameId, userId, DefaultEmailAddress);
            setup?.Invoke(invitation);
            return invitation;
        }

        protected static Game CreateGame() => new("organiser-001", "The Arena", DateTime.UtcNow, 60, 5);
    }

    public class Constructor : InvitationTestsBase
    {
        [Fact]
        public void CreatesInvitation_FromParameters()
        {
            var invitation = CreateInvitation(DefaultUserId);

            Assert.Equal(DefaultGameId, invitation.GameId);
            Assert.Equal(DefaultUserId, invitation.UserId);
            Assert.Equal(DefaultEmailAddress, invitation.EmailAddress);
            Assert.Equal(InvitationStatusEnum.None, invitation.Status);
            Assert.Null(invitation.ErrorMessage);
            Assert.Null(invitation.User);
        }

        [Fact]
        public void AllowsNullUserId()
        {
            var invitation = CreateInvitation();

            Assert.Null(invitation.UserId);
        }
    }

    public class GameProperty : InvitationTestsBase
    {
        [Fact]
        public void ThrowsUninitializedPropertyException_WhenGameNotSet()
        {
            var invitation = CreateInvitation();

            Assert.Throws<UninitializedPropertyException>(() => invitation.Game);
        }

        [Fact]
        public void ReturnsAssignedGame_WhenSetViaObjectInitializer()
        {
            var game = CreateGame();
            var invitation = new Invitation(DefaultGameId, DefaultUserId, DefaultEmailAddress) { Game = game };

            Assert.Equal(game, invitation.Game);
        }
    }

    public class Accept : InvitationTestsBase
    {
        [Fact]
        public void SetsStatusToAccepted_WhenPending()
        {
            var invitation = CreateInvitation();

            invitation.Accept();

            Assert.Equal(InvitationStatusEnum.Accepted, invitation.Status);
            Assert.True(invitation.IsDirty);
        }

        [Fact]
        public void DoesNothing_WhenAlreadyAccepted()
        {
            var invitation = CreateInvitation();
            invitation.Accept();

            invitation.Decline(); // attempt to flip it, should be a no-op

            Assert.Equal(InvitationStatusEnum.Accepted, invitation.Status);
        }

        [Fact]
        public void DoesNothing_WhenAlreadyDeclined()
        {
            var invitation = CreateInvitation();
            invitation.Decline();

            invitation.Accept();

            Assert.Equal(InvitationStatusEnum.Declined, invitation.Status);
        }

        [Fact]
        public void DoesNothing_WhenAlreadyFailed()
        {
            var invitation = CreateInvitation();
            invitation.DispatchError("Delivery failed.");

            invitation.Accept();

            Assert.Equal(InvitationStatusEnum.Failed, invitation.Status);
        }
    }

    public class Decline : InvitationTestsBase
    {
        [Fact]
        public void SetsStatusToDeclined_WhenPending()
        {
            var invitation = CreateInvitation();

            invitation.Decline();

            Assert.Equal(InvitationStatusEnum.Declined, invitation.Status);
            Assert.True(invitation.IsDirty);
        }

        [Fact]
        public void DoesNothing_WhenAlreadyDeclined()
        {
            var invitation = CreateInvitation();
            invitation.Decline();

            invitation.Accept(); // attempt to flip it, should be a no-op

            Assert.Equal(InvitationStatusEnum.Declined, invitation.Status);
        }

        [Fact]
        public void DoesNothing_WhenAlreadyAccepted()
        {
            var invitation = CreateInvitation();
            invitation.Accept();

            invitation.Decline();

            Assert.Equal(InvitationStatusEnum.Accepted, invitation.Status);
        }

        [Fact]
        public void DoesNothing_WhenAlreadyFailed()
        {
            var invitation = CreateInvitation();
            invitation.DispatchError("Delivery failed.");

            invitation.Decline();

            Assert.Equal(InvitationStatusEnum.Failed, invitation.Status);
        }
    }

    public class DispatchError : InvitationTestsBase
    {
        [Fact]
        public void SetsStatusToFailedAndRecordsMessage_WhenPending()
        {
            var invitation = CreateInvitation();

            invitation.DispatchError("Delivery failed.");

            Assert.Equal(InvitationStatusEnum.Failed, invitation.Status);
            Assert.Equal("Delivery failed.", invitation.ErrorMessage);
            Assert.True(invitation.IsDirty);
        }

        [Fact]
        public void DoesNothing_WhenAlreadyAccepted()
        {
            var invitation = CreateInvitation();
            invitation.Accept();

            invitation.DispatchError("Delivery failed.");

            Assert.Equal(InvitationStatusEnum.Accepted, invitation.Status);
            Assert.Null(invitation.ErrorMessage);
        }

        [Fact]
        public void DoesNothing_WhenAlreadyDeclined()
        {
            var invitation = CreateInvitation();
            invitation.Decline();

            invitation.DispatchError("Delivery failed.");

            Assert.Equal(InvitationStatusEnum.Declined, invitation.Status);
            Assert.Null(invitation.ErrorMessage);
        }

        [Fact]
        public void DoesNotOverwriteErrorMessage_WhenAlreadyFailed()
        {
            var invitation = CreateInvitation();
            invitation.DispatchError("First failure.");

            invitation.DispatchError("Second failure.");

            Assert.Equal("First failure.", invitation.ErrorMessage);
        }
    }

    public class AsSerializable : InvitationTestsBase
    {
        [Fact]
        public void IncludesIdGameIdAndDateCreated_WhenCalled()
        {
            var invitation = CreateInvitation(DefaultUserId);

            var serializable = invitation.AsSerializable();
            var type = serializable.GetType();

            Assert.Equal(invitation.Id, serializable.GetValue(type, "Id"));
            Assert.Equal(invitation.GameId, serializable.GetValue(type, "GameId"));
            Assert.Equal(invitation.DateCreated, serializable.GetValue(type, "DateCreated"));
        }

        [Fact]
        public void SetsInviteeToUserId_WhenUserIdIsSet()
        {
            var invitation = CreateInvitation(DefaultUserId);

            var serializable = invitation.AsSerializable();
            var type = serializable.GetType();

            Assert.Equal(DefaultUserId, serializable.GetValue(type, "Invitee"));
        }

        [Fact]
        public void SetsInviteeToEmailAddress_WhenUserIdIsNull()
        {
            var invitation = CreateInvitation();

            var serializable = invitation.AsSerializable();
            var type = serializable.GetType();

            Assert.Equal(DefaultEmailAddress, serializable.GetValue(type, "Invitee"));
        }

        [Fact]
        public void ExcludesEmailAddressAndStatus_WhenCalled()
        {
            var invitation = CreateInvitation(DefaultUserId);

            var serializable = invitation.AsSerializable();
            var type = serializable.GetType();

            Assert.Null(serializable.GetValue(type, "EmailAddress"));
            Assert.Null(serializable.GetValue(type, "Status"));
        }
    }
}