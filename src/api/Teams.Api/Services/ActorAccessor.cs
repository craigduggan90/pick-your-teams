using Teams.Api.Exceptions;
using Teams.Core.Models;
using Teams.Core.Services;

namespace Teams.Api.Services;

public class ActorAccessor(IHttpContextAccessor contextAccessor) : IActorAccessor
{
    private const string UserIdHeader = "Teams-User-Id";
    private const string UserTagHeader = "Teams-User-Tag";
    private const string UserNameHeader = "Teams-User-Name";

    public Actor Current => field ??= BuildActor();

    private Actor BuildActor()
    {
        var httpContext = contextAccessor.HttpContext
                          ?? throw new InvalidOperationException("No active HTTP context is available.");

        return new Actor(
            GetRequiredHeader(httpContext, UserIdHeader),
            GetRequiredHeader(httpContext, UserTagHeader),
            GetRequiredHeader(httpContext, UserNameHeader));
    }

    private static string GetRequiredHeader(HttpContext httpContext, string headerName)
    {
        if (!httpContext.Request.Headers.TryGetValue(headerName, out var values)
            || values.Count == 0
            || string.IsNullOrWhiteSpace(values[0]))
        {
            throw new MissingHeaderException(headerName);
        }

        return values[0]!;
    }
}