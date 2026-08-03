using Microsoft.AspNetCore.Mvc.Filters;
using Teams.Api.Exceptions;
using Teams.Common;

namespace Teams.Api.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequiresScopeAttribute(string scope) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(Constants.ScopeHeaderKey, out var values)
            || !values.ToString()
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Contains(scope, StringComparer.OrdinalIgnoreCase))
        {
            throw new MissingScopeException(scope);
        }
    }
}