namespace Teams.Authoriser.Auth;

public interface IUserInfoClient
{
    /// <summary>Null on any non-success response.</summary>
    Task<UserInfo?> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken);
}