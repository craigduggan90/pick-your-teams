namespace Teams.Api.Infrastructure;

public static class Scopes
{
    public static class Jobs
    {
        public const string Read = "jobs:read";
        public const string Enqueue = "jobs:enqueue";
        public const string Modify = "jobs:modify";
    }
}