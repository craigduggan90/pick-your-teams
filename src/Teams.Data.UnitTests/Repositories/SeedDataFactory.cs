using Teams.Common.Providers.Identifiers;
using Teams.Common.Providers.Temporal;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories;

public static class SeedDataFactory
{
    private static readonly DateTimeOffset BaseDate = new(2020, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public static class Users
    {
        public static string GetIdentifier(int index) => $"u-{index:D8}";

        public static User Create(int index)
        {
            var id = GetIdentifier(index);
            using var idFix = new IdentifierProviderContext(id);
            using var createdDtFix = new DateTimeOffsetProviderContext(BaseDate.AddDays(index));

            var displayName = $"display name {index:D8}";
            var externalId = $"test|{id}";
            var emailAddress = $"{id}@test.io";
            var tag = $"tag-{index:D8}";
            var mobile = index % 2 == 0 ? null : $"07{index:D9}";
            var ratingChange = index * (index % 2 == 0 ? 10 : -10);

            var user = new User(displayName, externalId, emailAddress, null);

            using var updatedDtFix = new DateTimeOffsetProviderContext(BaseDate.AddYears(2).AddDays(index));
            user.Update(tag, null, null, mobile);
            user.ApplyRatingChange(ratingChange);

            return user;
        }
    }

    public static class Games
    {
        private static readonly string?[] Locations = [null, "Indoor", "Outdoor", "Outer Space"];
        private static readonly GameTeamEnum?[] Winners = [null, GameTeamEnum.Home, GameTeamEnum.Away, GameTeamEnum.None];
        private static readonly int[] TeamSizes = [3, 5, 7, 11];
        private static readonly int[] Durations = [30, 45, 60, 90, 120];

        public static string GetIdentifier(int index) => $"g-{index:D8}";

        public static Game Create(int index, User organiser)
        {
            using var idFix = new IdentifierProviderContext(GetIdentifier(index));
            using var createdDtFix = new DateTimeOffsetProviderContext(BaseDate.AddDays(index));

            var location = Locations[index % Locations.Length];
            var teamSize = TeamSizes[index % TeamSizes.Length];
            var duration = Durations[index % Durations.Length];
            var startTime = BaseDate.UtcDateTime.AddDays(index);

            // Create the base game in a state that we know will be modified
            var game = new Game(organiser.Id, location, startTime, 999, teamSize)
            {
                Organiser = organiser
            };

            using var updatedDtFix = new DateTimeOffsetProviderContext(BaseDate.AddYears(2).AddDays(index));
            game.Update(null, null, duration);

            var winner = Winners[index % Winners.Length];
            if (winner is not null)
                game.SetResult(winner.Value);

            return game;
        }
    }

    public static class Players
    {
        public static string GetIdentifier(string gameId, int index) => $"{gameId}-p-{index:D8}";

        public static Player CreateDummy(int index, Game game, GameTeamEnum team = GameTeamEnum.None)
        {
            var baseDateTimeOffset = new DateTimeOffset(game.DateCreated, TimeSpan.Zero);
            using var idFix = new IdentifierProviderContext(GetIdentifier(game.Id, index));
            using var createdDtFix = new DateTimeOffsetProviderContext(baseDateTimeOffset.AddSeconds(index));
            var ratingChange = index * (index % 2 == 0 ? 10 : -10);
            var player = new Player(game, $"display-name {index:D8}", 1000);

            using var modifiedDtFix = new DateTimeOffsetProviderContext(baseDateTimeOffset.AddHours(2).AddSeconds(index));
            player.AssignTeam(team, player.Rating + ratingChange);
            return player;
        }
    }
}