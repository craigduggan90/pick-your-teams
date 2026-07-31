using Auth0.ManagementApi;
using Teams.Core.Exceptions;

namespace Teams.Core.Services.IdentityProvider;

public class IdpClient(
    IManagementApiClient idpClient,
    IdpSettings settings)
    : IIdpClient
{
    public async Task<string> CreateUser(
        string tag,
        string? emailAddress,
        string? mobile,
        CancellationToken cancellationToken)
    {
        var request = new CreateUserRequestContent
        {
            Connection = "google-oauth2",
            Email = emailAddress,
            PhoneNumber = mobile,
            Username = tag
        };

        var options = new RequestOptions();

        var user = await idpClient.Users.CreateAsync(request, options, cancellationToken);
        return user.UserId ?? throw new IdpException("Failed to create user");
    }
}