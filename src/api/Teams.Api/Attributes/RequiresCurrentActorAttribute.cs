using Microsoft.AspNetCore.Mvc.Filters;
using Teams.Core.Services;

namespace Teams.Api.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequiresCurrentActorAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var actorAccessor = context.HttpContext.RequestServices.GetRequiredService<IActorAccessor>();
        _ = actorAccessor.Current;
    }
}