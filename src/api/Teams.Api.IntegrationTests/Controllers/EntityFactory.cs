using Teams.Common.Providers.Identifiers;
using Teams.Common.Providers.Temporal;
using Teams.Domain.Entities;
using Teams.Domain.Entities.Abstract;

namespace Teams.Api.IntegrationTests.Controllers;

internal static class EntityFactory
{
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
            dateCreated ?? DateTimeOffset.UtcNow,
            () => new User(
                displayName ?? "Test User",
                externalId ?? $"external-{Guid.NewGuid():N}",
                email ?? $"{Guid.NewGuid():N}@test.net",
                mobile),
            postCreationSteps);
}