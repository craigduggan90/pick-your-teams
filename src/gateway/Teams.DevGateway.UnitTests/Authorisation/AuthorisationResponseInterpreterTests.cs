using Amazon.Lambda.APIGatewayEvents;
using Teams.DevGateway.Authorisation;

namespace Teams.DevGateway.UnitTests.Authorisation;

public class AuthorisationResponseInterpreterTests
{
    private static APIGatewayCustomAuthorizerResponse ResponseWithEffect(string? effect) => new()
    {
        PolicyDocument = new APIGatewayCustomAuthorizerPolicy
        {
            Statement =
            [
                new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement { Effect = effect! },
            ],
        },
    };

    [Fact]
    public void IsAllowed_is_true_for_an_allow_statement()
    {
        Assert.True(AuthorisationResponseInterpreter.IsAllowed(ResponseWithEffect("Allow")));
    }

    [Theory]
    [InlineData("allow")]
    [InlineData("ALLOW")]
    public void IsAllowed_is_case_insensitive(string effect)
    {
        Assert.True(AuthorisationResponseInterpreter.IsAllowed(ResponseWithEffect(effect)));
    }

    [Fact]
    public void IsAllowed_is_false_for_a_deny_statement()
    {
        Assert.False(AuthorisationResponseInterpreter.IsAllowed(ResponseWithEffect("Deny")));
    }

    [Fact]
    public void IsAllowed_is_false_when_policy_document_is_missing()
    {
        Assert.False(AuthorisationResponseInterpreter.IsAllowed(new APIGatewayCustomAuthorizerResponse()));
    }

    [Fact]
    public void IsAllowed_is_false_when_statement_list_is_empty()
    {
        var response = new APIGatewayCustomAuthorizerResponse
        {
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy(),
        };

        Assert.False(AuthorisationResponseInterpreter.IsAllowed(response));
    }
}