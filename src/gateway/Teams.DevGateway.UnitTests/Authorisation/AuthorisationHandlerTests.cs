using Amazon.Lambda.APIGatewayEvents;
using NSubstitute;
using Teams.DevGateway.Authorisation;

namespace Teams.DevGateway.UnitTests.Authorisation;

public class AuthorisationHandlerTests
{
    private static APIGatewayCustomAuthorizerResponse ResponseWithEffect(string effect) => new()
    {
        PolicyDocument = new APIGatewayCustomAuthorizerPolicy
        {
            Statement = [new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement { Effect = effect }],
        },
    };

    [Fact]
    public async Task DecideAsync_allows_when_the_authoriser_returns_an_allow_effect()
    {
        var authoriserClient = Substitute.For<IAuthoriserClient>();
        authoriserClient.AuthorizeAsync(Arg.Any<APIGatewayCustomAuthorizerRequest>(), Arg.Any<CancellationToken>())
            .Returns(ResponseWithEffect("Allow"));

        var handler = new AuthorisationHandler(authoriserClient);

        var decision = await handler.DecideAsync("Bearer abc", "/users/self", "GET", CancellationToken.None);

        Assert.True(decision.IsAllowed);
    }

    [Fact]
    public async Task DecideAsync_denies_when_the_authoriser_returns_a_deny_effect()
    {
        var authoriserClient = Substitute.For<IAuthoriserClient>();
        authoriserClient.AuthorizeAsync(Arg.Any<APIGatewayCustomAuthorizerRequest>(), Arg.Any<CancellationToken>())
            .Returns(ResponseWithEffect("Deny"));

        var handler = new AuthorisationHandler(authoriserClient);

        var decision = await handler.DecideAsync(null, "/users/self", "GET", CancellationToken.None);

        Assert.False(decision.IsAllowed);
    }

    [Fact]
    public async Task DecideAsync_forwards_the_authorization_header_and_request_details_to_the_authoriser()
    {
        APIGatewayCustomAuthorizerRequest? capturedRequest = null;

        var authoriserClient = Substitute.For<IAuthoriserClient>();
        authoriserClient.AuthorizeAsync(Arg.Do<APIGatewayCustomAuthorizerRequest>(r => capturedRequest = r), Arg.Any<CancellationToken>())
            .Returns(ResponseWithEffect("Deny"));

        var handler = new AuthorisationHandler(authoriserClient);
        await handler.DecideAsync("Bearer xyz", "/games/1", "POST", CancellationToken.None);

        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Headers);
        Assert.Equal("Bearer xyz", capturedRequest.Headers["Authorization"]);
        Assert.Equal("/games/1", capturedRequest.Path);
        Assert.Equal("POST", capturedRequest.HttpMethod);
    }
}
