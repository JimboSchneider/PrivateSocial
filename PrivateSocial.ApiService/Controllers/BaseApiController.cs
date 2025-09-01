using Microsoft.AspNetCore.Mvc;

namespace PrivateSocial.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected ILogger Logger { get; }

    protected BaseApiController(ILogger logger)
    {
        Logger = logger;
    }
}