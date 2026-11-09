using AdmissionSystem.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Api.Controllers;

public record CreateClassRequest(string Name, int TotalSeats);

[ApiController]
[Route("classes")]
public class ClassesController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateClassRequest req)
    {
        var c = new SchoolClass { Name = req.Name, TotalSeats = req.TotalSeats };
        db.Classes.Add(c);
        await db.SaveChangesAsync();
        return Created($"/classes/{c.Id}", new { c.Id, c.Name, c.TotalSeats });
    }

    [HttpGet]
    public async Task<IActionResult> List() =>
        Ok(await db.Classes.Select(c => new { c.Id, c.Name, c.TotalSeats }).ToListAsync());
}