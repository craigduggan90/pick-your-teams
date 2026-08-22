using Teams.Authoriser.Auth;

namespace Teams.Authoriser.UnitTests.Auth;

public class AuthorizerPolicyFactoryTests
{
    [Fact]
    public void Deny_builds_a_deny_effect_policy_for_the_given_resource()
    {
        var response = AuthorizerPolicyFactory.Deny("arn:aws:execute-api:region:account:api-id/stage/GET/users/self");

        var statement = Assert.Single(response.PolicyDocument.Statement);
        Assert.Equal("Deny", statement.Effect);
        Assert.Contains("execute-api:Invoke", statement.Action);
        Assert.Contains("arn:aws:execute-api:region:account:api-id/stage/GET/users/self", statement.Resource);
        Assert.Equal("2012-10-17", response.PolicyDocument.Version);
    }

    [Fact]
    public void Deny_falls_back_to_a_wildcard_resource_when_methodArn_is_missing()
    {
        var response = AuthorizerPolicyFactory.Deny(null);

        var statement = Assert.Single(response.PolicyDocument.Statement);
        Assert.Contains("*", statement.Resource);
    }
}