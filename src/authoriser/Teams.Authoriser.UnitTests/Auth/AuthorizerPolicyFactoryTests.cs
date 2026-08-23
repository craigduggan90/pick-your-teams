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

    [Fact]
    public void Deny_has_no_context()
    {
        var response = AuthorizerPolicyFactory.Deny(null);

        Assert.Null(response.Context);
    }

    [Fact]
    public void Allow_builds_an_allow_effect_policy_for_the_given_resource()
    {
        var user = new ResolvedUser("u1", "u1_tag", "Jane Smith");

        var response = AuthorizerPolicyFactory.Allow(
            "arn:aws:execute-api:region:account:api-id/stage/GET/users/self", user);

        var statement = Assert.Single(response.PolicyDocument.Statement);
        Assert.Equal("Allow", statement.Effect);
        Assert.Contains("execute-api:Invoke", statement.Action);
        Assert.Contains("arn:aws:execute-api:region:account:api-id/stage/GET/users/self", statement.Resource);
        Assert.Equal(user.Id, response.PrincipalID);
    }

    [Fact]
    public void Allow_carries_the_resolved_user_and_an_empty_scopes_value_in_context()
    {
        var user = new ResolvedUser("u1", "u1_tag", "Jane Smith");

        var response = AuthorizerPolicyFactory.Allow(null, user);

        Assert.NotNull(response.Context);
        Assert.Equal("u1", response.Context["Teams-User-Id"]);
        Assert.Equal("u1_tag", response.Context["Teams-User-Tag"]);
        Assert.Equal("Jane Smith", response.Context["Teams-User-Name"]);
        Assert.Equal("", response.Context["Scopes"]);
    }
}