using Microsoft.AspNetCore.Mvc;
using Teams.Api.Controllers.V1.Abstract;

namespace Teams.Api.Controllers.V1;

public class HealthController : ApiControllerBase
{
    [HttpGet, ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Ping()
        => NoContent();
}