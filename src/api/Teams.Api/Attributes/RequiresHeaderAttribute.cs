using Microsoft.AspNetCore.Mvc.Filters;
using Teams.Api.Exceptions;

namespace Teams.Api.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequiresHeaderAttribute(string headerName) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(headerName, out var values)
            || values.Count == 0
            || string.IsNullOrWhiteSpace(values[0]))
        {
            throw new MissingHeaderException(headerName);
        }
    }
}