using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Responses;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/v1/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(ApiResponseFactory.Success(
            new
            {
                status = "ok",
                utcNow = DateTime.UtcNow
            },
            "API foundation is running."));
    }
}
