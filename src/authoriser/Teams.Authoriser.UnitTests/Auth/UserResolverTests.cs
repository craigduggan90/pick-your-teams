using NSubstitute;
using Teams.Authoriser.Auth;

namespace Teams.Authoriser.UnitTests.Auth;

public class UserResolverTests
{
    private readonly ITeamsApiClient teamsApiClient = Substitute.For<ITeamsApiClient>();
    private readonly IUserInfoClient userInfoClient = Substitute.For<IUserInfoClient>();

    private UserResolver CreateSut() => new(teamsApiClient, userInfoClient);

    [Fact]
    public async Task ResolveAsync_returns_the_existing_user_without_calling_userinfo_or_create()
    {
        teamsApiClient.GetByExternalIdAsync("external-id", Arg.Any<CancellationToken>())
            .Returns(new TeamsUser("u1", "u1", "Jane Smith", 1042));

        var sut = CreateSut();
        var result = await sut.ResolveAsync("external-id", "access-token", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("u1", result.Id);
        Assert.Equal("Jane Smith", result.DisplayName);

        await userInfoClient.DidNotReceive().GetUserInfoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await teamsApiClient.DidNotReceive().CreateAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_creates_a_user_from_userinfo_when_none_exists()
    {
        teamsApiClient.GetByExternalIdAsync("external-id", Arg.Any<CancellationToken>()).Returns((TeamsUser?)null);
        userInfoClient.GetUserInfoAsync("access-token", Arg.Any<CancellationToken>())
            .Returns(new UserInfo("Jane Smith", "jane@example.com"));
        teamsApiClient.CreateAsync("Jane Smith", "external-id", "jane@example.com", Arg.Any<CancellationToken>())
            .Returns(new TeamsUser("u2", "u2", "Jane Smith", 1000));

        var sut = CreateSut();
        var result = await sut.ResolveAsync("external-id", "access-token", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("u2", result.Id);
        Assert.Equal("u2", result.Tag);
        Assert.Equal("Jane Smith", result.DisplayName);
    }

    [Theory]
    [InlineData(null, "jane@example.com")]
    [InlineData("", "jane@example.com")]
    [InlineData("Jane Smith", null)]
    [InlineData("Jane Smith", "")]
    public async Task ResolveAsync_returns_null_when_userinfo_is_missing_name_or_email(string? name, string? email)
    {
        teamsApiClient.GetByExternalIdAsync("external-id", Arg.Any<CancellationToken>()).Returns((TeamsUser?)null);
        userInfoClient.GetUserInfoAsync("access-token", Arg.Any<CancellationToken>())
            .Returns(new UserInfo(name, email));

        var sut = CreateSut();
        var result = await sut.ResolveAsync("external-id", "access-token", CancellationToken.None);

        Assert.Null(result);
        await teamsApiClient.DidNotReceive().CreateAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_returns_null_when_userinfo_call_fails()
    {
        teamsApiClient.GetByExternalIdAsync("external-id", Arg.Any<CancellationToken>()).Returns((TeamsUser?)null);
        userInfoClient.GetUserInfoAsync("access-token", Arg.Any<CancellationToken>()).Returns((UserInfo?)null);

        var sut = CreateSut();
        var result = await sut.ResolveAsync("external-id", "access-token", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_returns_null_when_the_lookup_throws()
    {
        teamsApiClient.GetByExternalIdAsync("external-id", Arg.Any<CancellationToken>())
            .Returns<TeamsUser?>(_ => throw new HttpRequestException("network down"));

        var sut = CreateSut();
        var result = await sut.ResolveAsync("external-id", "access-token", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_returns_null_when_create_throws()
    {
        teamsApiClient.GetByExternalIdAsync("external-id", Arg.Any<CancellationToken>()).Returns((TeamsUser?)null);
        userInfoClient.GetUserInfoAsync("access-token", Arg.Any<CancellationToken>())
            .Returns(new UserInfo("Jane Smith", "jane@example.com"));
        teamsApiClient.CreateAsync("Jane Smith", "external-id", "jane@example.com", Arg.Any<CancellationToken>())
            .Returns<TeamsUser>(_ => throw new HttpRequestException("validation failed"));

        var sut = CreateSut();
        var result = await sut.ResolveAsync("external-id", "access-token", CancellationToken.None);

        Assert.Null(result);
    }
}