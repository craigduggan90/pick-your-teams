using Teams.Data.Repositories.Users;

namespace Teams.Api.EndToEndTests.TestMiddleware;

public class ActorResolverMiddleware(RequestDelegate next)
{
    private const string UserIdHeader = "Teams-User-Id";
    private const string UserTagHeader = "Teams-User-Tag";
    private const string UserNameHeader = "Teams-User-Name";
    private const string BearerPrefix = "Bearer ";

    public async Task InvokeAsync(HttpContext context, IReadOnlyUsersRepository usersRepository)
    {
        var authorizationHeader = context.Request.Headers.Authorization.ToString();

        if (authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var id = authorizationHeader[BearerPrefix.Length..].Trim();
            
            var user = await usersRepository.GetByIdAsync(id, context.RequestAborted);

            if (user is not null)
            {
                context.Request.Headers[UserIdHeader] = user.Id;
                context.Request.Headers[UserTagHeader] = user.Tag;
                context.Request.Headers[UserNameHeader] = user.DisplayName;
            }
        }

        await next(context);
    }
}
