using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Teams.Common;

namespace Teams.Api.Controllers.V1.Abstract;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]", Order = 0)]
[Route("api/[controller]", Order = 1)]
public abstract class ApiControllerBase : ControllerBase
{
    protected void SetEtagResponseHeader(string value)
        => SetResponseHeader(Constants.ETagHeaderKey, $"\"{value}\"");

    private void SetResponseHeader(string key, string value)
        => Response.Headers.TryAdd(key, value);
}