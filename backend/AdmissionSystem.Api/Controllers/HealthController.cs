using AdmissionSystem.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace AdmissionSystem.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(new { api = "ok", db = await db.Database.CanConnectAsync() ? "ok" : "down" });
}