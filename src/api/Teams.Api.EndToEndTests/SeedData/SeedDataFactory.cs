using Teams.Common.Providers.Identifiers;
using Teams.Common.Providers.Temporal;
using Teams.Domain.Entities;

namespace Teams.Api.EndToEndTests.SeedData;

public static class SeedDataFactory
{
    public static User[] SeedUsers => field ??= GetSeedUsers();

    private static readonly DateTimeOffset BaseDate = DateTimeOffsetProvider.Now;

    private sealed record UserDetail(int Seed, string UserName, string? Tag);

    private static UserDetail[] UserDetails =>
    [
        new(1, "Rob Lee", "rob-lee-7" ),
        new(2, "Steve Howey", "howey_the_lads"),
        new(3, "Peter Beardsley", "weeman"),
        new(4, "Pavel Srnicek", "pavelisageordie"),
        new(5, "Lee Clark", "lee"),
        new(6, "John Beresford", "jbes"),
        new(7, "Liam O'Brien", "surprise169"),
        new(8, "Alan Shearer", "nine"),
        new(9, "Warren Barton", "wazza"),
        new(10, "Scott Sellars", null),
        new(11, "Kevin Scott", null),
        new(12, "Steve Watson", "watto"),
        new(13, "Darren Peacock", "DazzaP"),
        new(14, "Gary Kelly", "KELLY"),
        new(15, "Mark Robinson", "robbo"),
        new(16, "Brian Kilcline", "Kilcline"),
        new(17, "Micky Quinn", "quinn"),
        new(18, "Andy Cole", "COLEmeansGOAL"),
        new(19, "Les Ferdinand", "les"),
        new(20, "Phillipe Albert", "lob"),
        new(21, "David Kelly", "worDave"),
        new(22, "Gavin Peacock", "HiMyNameIsDrew"),
        new(23, "Alecbie Matthew", "Alecbie"),
        new(24, "Keith Gillespie", "gillespie"),
        new(25, "David Ginola", "beforeTheNameChange")
    ];

    private static User[] GetSeedUsers() =>
        UserDetails.Select(tuple =>
            {
                tuple.Deconstruct(out var seed, out var name, out var tag);
                using var idFix = new IdentifierProviderContext($"user-{seed:D3}");
                using var dtFix = new DateTimeOffsetProviderContext(BaseDate.AddSeconds(seed));
                var user = new User(name, $"auth|{seed:D36}", $"player-{seed:D3}@test.net", null);
                user.Update(tag, null, null, null);
                return user;
            })
            .ToArray();
}