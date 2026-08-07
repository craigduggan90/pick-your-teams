namespace Teams.Common;

public static class Constants
{
    public const string IdempotencyHeaderKey = "Idempotency-Key";
    public const string IfMatchHeaderKey = "If-Match";
    public const string ETagHeaderKey = "ETag";
    public const string ApiVersionHeaderKey = "Api-Version";
    public const string ScopeHeaderKey = "Scopes";
    public const string TagRegexPattern = "^(?=.*[A-Za-z0-9])[A-Za-z0-9_][A-Za-z0-9._-]*$";
}