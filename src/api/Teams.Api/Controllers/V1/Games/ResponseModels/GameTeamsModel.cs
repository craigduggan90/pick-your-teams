using System.Diagnostics.CodeAnalysis;

namespace Teams.Api.Controllers.V1.Games.ResponseModels;

public record GameTeamsModel(string Id, GameTeamModel? Home, GameTeamModel? Away)
{
    [ExcludeFromCodeCoverage]
    public static GameTeamsModel Example => new(
        "d40b639aa73b427f9eb47da3491b9097",
        GameTeamModel.HomeTeamExample,
        GameTeamModel.AwayTeamExample);

    [ExcludeFromCodeCoverage]
    public static GameTeamsModel Empty => new(
        "d40b639aa73b427f9eb47da3491b9097",
        null,
        null);
}

public record GameTeamModel(IReadOnlyCollection<GameTeamPlayerModel> Players, int TeamRating)
{
    [ExcludeFromCodeCoverage]
    public static GameTeamModel HomeTeamExample => new(
        Players:
        [
            new GameTeamPlayerModel("f2b364bb415a44ecb2b51c177b3473d3", "James 'Tha Carter' Carter", "thacarter", 855),
            new GameTeamPlayerModel("5c0a8fdcc96744fbac0260771ba60cdd", "Oliver Reed", "oliverreed", 794),
            new GameTeamPlayerModel("f1006bc92efa4bd5a36697de2ac1d36d", "Noah Turner", "noahturner", 1189),
            new GameTeamPlayerModel("8d97460961b1403994f338833f8783f9", "Harry Collins", null, 905),
            new GameTeamPlayerModel("075c318a469741f9b8a21b298fa3ec61", "Ethan Brooks", "ethanbrooks", 1141)
        ],
        TeamRating: 4884);

    [ExcludeFromCodeCoverage]
    public static GameTeamModel AwayTeamExample => new(
    Players: [
        new GameTeamPlayerModel("6034b2566e5940d48e9a97f040e58ea6", "Freddie Bennett", "freddiebennett", 1034),
        new GameTeamPlayerModel("b0ff5d5e33d74f0582ac347cf17444d3", "Leo Martin", "leomartin", 880),
        new GameTeamPlayerModel("39e4cd5b905a405380f9b76fac19dc51", "Archie White", "archiewhite", 1289),
        new GameTeamPlayerModel("5d2312007c4744a1b38dbcfb179c249c", "Theo Arvin", null, 1361),
        new GameTeamPlayerModel("b6300cb3623c4f7aaf6af910eb5054c3", "Finn Hale", "finnhale", 951)
    ], TeamRating: 5515);
}

public record GameTeamPlayerModel(string Id, string? DisplayName, string? Tag, int Rating);