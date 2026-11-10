using AdmissionSystem.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Api.Controllers;

public record CreateClassRequest(string? Name, int TotalSeats);

public record ClassResponse(Guid Id, string Name, int TotalSeats);

public record ClassSeatsResponse(
    Guid Id, string Name, int TotalSeats,
    int HeldSeats, int ConfirmedSeats, int AvailableSeats);

[ApiController]
[Route("classes")]
public class ClassesController(AppDbContext db) : ControllerBase
{
    private const int MaxNameLength = 200;
    private const int MaxSeats = 10_000;

    [HttpPost]
    public async Task<IActionResult> Create(CreateClassRequest req)
    {
        var name = req.Name?.Trim() ?? "";
        if (name.Length == 0)
            return Problem("Class name is required.", statusCode: 400);
        if (name.Length > MaxNameLength)
            return Problem($"Class name must be at most {MaxNameLength} characters.", statusCode: 400);
        if (req.TotalSeats is < 1 or > MaxSeats)
            return Problem($"Number of seats must be between 1 and {MaxSeats}.", statusCode: 400);

        var c = new SchoolClass { Name = name, TotalSeats = req.TotalSeats };
        db.Classes.Add(c);
        await db.SaveChangesAsync();
        return Created($"/classes/{c.Id}", new ClassResponse(c.Id, c.Name, c.TotalSeats));
    }

    [HttpGet]
    public async Task<IActionResult> List() =>
        Ok(await WithSeatCounts(db.Classes.OrderBy(c => c.Name)));

    [HttpGet("{classId:guid}")]
    public async Task<IActionResult> Get(Guid classId)
    {
        var result = await WithSeatCounts(db.Classes.Where(c => c.Id == classId));
        return result.Count == 0
            ? Problem("Class not found.", statusCode: 404)
            : Ok(result[0]);
    }
    private static async Task<List<ClassSeatsResponse>> WithSeatCounts(IQueryable<SchoolClass> classes)
    {
        var now = DateTime.UtcNow;
        var rows = await classes
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.TotalSeats,
                Held = c.Holds.Count(h => h.Status == HoldStatus.HELD && h.ExpiresAt > now),
                Confirmed = c.Holds.Count(h => h.Status == HoldStatus.CONFIRMED),
            })
            .ToListAsync();

        return rows.Select(r => new ClassSeatsResponse(
            r.Id, r.Name, r.TotalSeats, r.Held, r.Confirmed,
            Math.Max(0, r.TotalSeats - r.Held - r.Confirmed))).ToList();
    }
}
