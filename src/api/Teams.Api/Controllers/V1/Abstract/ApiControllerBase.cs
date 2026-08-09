using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Teams.Api.Controllers.V1.Abstract;

[ApiController]
[ApiVersion(1.0)]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]", Order = 0)]
[Route("api/[controller]", Order = 1)]
public abstract class ApiControllerBase : ControllerBase;