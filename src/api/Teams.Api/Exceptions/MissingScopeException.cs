namespace Teams.Api.Exceptions;

public class MissingScopeException(params string[] scopes) : Exception(scopes.Length == 1
        ? $"Required scope was not present: '{scopes.Single()}'."
        : $"Required scopes were not present: {string.Join(", ", scopes.Select(scope => $"'{scope}'"))}.");