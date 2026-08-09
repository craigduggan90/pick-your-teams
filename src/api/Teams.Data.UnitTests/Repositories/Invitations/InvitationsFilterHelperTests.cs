using Teams.Common.Providers.Identifiers;
using Teams.Data.Repositories.Invitations;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories.Invitations;

public static class InvitationsFilterHelperTests
{
    private static readonly User TargetUser = CreateUser("target-user");
    private static readonly User OtherUser = CreateUser("other-user");
    private static readonly Game GameA = CreateGame("game-a", "organiser-a");
    private static readonly Game GameB = CreateGame("game-b", "organiser-b");

    private static User CreateUser(string id)
    {
        using var idFix = new IdentifierProviderContext(id);
        return new User($"display {id}", $"ext|{id}", $"{id}@test.io", null);
    }

    private static Game CreateGame(string id, string organiserId)
    {
        using var idFix = new IdentifierProviderContext(id);
        return new Game(organiserId, "location", DateTime.UtcNow, 60, 5);
    }

    private static IQueryable<Invitation> GetSeedData(int count) =>
        Enumerable.Range(1, count)
            .Select(i =>
            {
                var game = i % 2 == 0 ? GameA : GameB;
                var user = i % 3 == 0 ? TargetUser : i % 5 == 0 ? OtherUser : null;
                return SeedDataFactory.Invitations.Create(i, game, user);
            })
            .AsQueryable();

    public class ApplyEmailAddressFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyEmailAddressFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            var data = GetSeedData(30);
            var value = data.Skip(14).First().EmailAddress;
            var expected = data.Where(invitation => invitation.EmailAddress.Contains(value));
            var filtered = data.ApplyEmailAddressFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyGameIdFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyGameIdFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            var data = GetSeedData(30);
            var expected = data.Where(invitation => invitation.GameId == GameA.Id);
            var filtered = data.ApplyGameIdFilter(GameA.Id);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyUserIdFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyUserIdFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            var data = GetSeedData(30);
            var expected = data.Where(invitation => invitation.UserId == TargetUser.Id);
            var filtered = data.ApplyUserIdFilter(TargetUser.Id);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyStatusFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyStatusFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const InvitationStatusEnum value = InvitationStatusEnum.Accepted;
            var data = GetSeedData(30);
            var expected = data.Where(invitation => invitation.Status == value);
            var filtered = data.ApplyStatusFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }
}