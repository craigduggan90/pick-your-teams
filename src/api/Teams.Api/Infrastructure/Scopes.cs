namespace Teams.Api.Infrastructure;

public static class Scopes
{
    /// <summary>Held only by Teams.Authoriser, for its own pre-identity calls to resolve or create a User.
    /// Never granted to an end-user request - Teams.DevGateway (and, in production, API Gateway) strip this
    /// header from anything the UI sends before it ever reaches here.</summary>
    public const string Authoriser = "authoriser";

    public static class Jobs
    {
        public const string Read = "jobs:read";
        public const string Enqueue = "jobs:enqueue";
        public const string Modify = "jobs:modify";
    }
}