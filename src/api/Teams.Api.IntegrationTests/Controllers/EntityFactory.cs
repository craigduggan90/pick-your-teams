using Teams.Common.Providers.Identifiers;
using Teams.Common.Providers.Temporal;
using Teams.Domain.Entities;
using Teams.Domain.Entities.Abstract;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers;

internal static class EntityFactory
{
    // Cursor has a DB-level unique constraint and is derived from an entity's created timestamp. DateTimeOffset.UtcNow
    // sampled independently per call is not guaranteed unique - two calls in quick succession (or under parallel test
    // execution) can land in the same microsecond. This is a monotonic, thread-safe substitute: every call without an
    // explicit dateCreated gets a strictly increasing timestamp, so uniqueness holds regardless of clock resolution.
    // A single fixed base plus a pure arithmetic offset (rather than re-sampling UtcNow per call) keeps the guarantee
    // independent of wall-clock behavior entirely.
    private static readonly DateTimeOffset SequenceBase = DateTimeOffset.UtcNow;
    private static long _sequence;

    private static DateTimeOffset NextDateCreated() =>
        SequenceBase.AddTicks(Interlocked.Increment(ref _sequence) * TimeSpan.TicksPerMicrosecond);

    public static T CreateSeeded<T>(string id, DateTimeOffset dateCreated, Func<T> factory, Action<T>? postCreation = null)
        where T : EntityBase
    {
        using var idFix = new IdentifierProviderContext(id);
        using var dtFix = new DateTimeOffsetProviderContext(dateCreated);
        var t = factory();
        postCreation?.Invoke(t);
        return t;
    }

    public static User CreateUser(
        string? id = null,
        string? displayName = null,
        string? externalId = null,
        string? email = null,
        string? mobile = null,
        DateTimeOffset? dateCreated = null,
        Action<User>? postCreationSteps = null) =>
        CreateSeeded(
            id ?? $"user-{Guid.NewGuid():N}",
            dateCreated ?? NextDateCreated(),
            () => new User(
                displayName ?? "Test User",
                externalId ?? $"external-{Guid.NewGuid():N}",
                email ?? $"{Guid.NewGuid():N}@test.net",
                mobile),
            postCreationSteps);

    /// <summary>Creates a seeded game. <paramref name="organiserId"/> is required (not defaulted) because a game's
    /// organiser must reference a real seeded <see cref="User"/> - callers should seed that user first.</summary>
    public static Game CreateGame(
        string organiserId,
        string? id = null,
        string? location = "Test Venue",
        DateTime? startTime = null,
        int duration = 60,
        int teamSize = 5,
        DateTimeOffset? dateCreated = null,
        Action<Game>? postCreationSteps = null) =>
        CreateSeeded(
            id ?? $"game-{Guid.NewGuid():N}",
            dateCreated ?? NextDateCreated(),
            () => new Game(organiserId, location, startTime ?? DateTime.UtcNow, duration, teamSize),
            postCreationSteps);

    /// <summary>Creates a seeded player. <paramref name="gameId"/> is required (not defaulted) because a player must
    /// reference a real seeded <see cref="Game"/> - callers should seed that game first.
    /// <para>Defaults to a <see cref="PlayerTypeEnum.Dummy"/> player with no linked user - pass
    /// <paramref name="userId"/> (and <c>type: PlayerTypeEnum.User</c>) explicitly for a "real" user-backed player,
    /// since most players in practice are not linked to a user.</para></summary>
    public static Player CreatePlayer(
        string gameId,
        string? id = null,
        string? userId = null,
        string? displayName = null,
        int rating = 1000,
        PlayerTypeEnum type = PlayerTypeEnum.Dummy,
        GameTeamEnum team = GameTeamEnum.None,
        DateTimeOffset? dateCreated = null,
        Action<Player>? postCreationSteps = null) =>
        CreateSeeded(
            id ?? $"player-{Guid.NewGuid():N}",
            dateCreated ?? NextDateCreated(),
            () => new Player(gameId, userId, displayName ?? "Test Player", rating, type, team),
            postCreationSteps);
}