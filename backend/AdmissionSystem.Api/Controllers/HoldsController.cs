using AdmissionSystem.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Api.Controllers;

public record HoldCreatedResponse(
    Guid HoldId, Guid ClassId, string StudentId, string Status, DateTime ExpiresAt);

public record HoldResponse(
    Guid HoldId, Guid ClassId, string ClassName, string StudentId, string Status, DateTime ExpiresAt);

public record ConfirmResponse(Guid HoldId, string Status);

[ApiController]
public class HoldsController(AppDbContext db, HoldSettings settings) : ControllerBase
{
    public const string StudentIdHeader = "X-Student-Id";
    private const int StudentIdLength = 17;

    [HttpPost("/classes/{classId:guid}/holds")]
    public async Task<IActionResult> Create(Guid classId)
    {
        var studentId = Request.Headers[StudentIdHeader].ToString().Trim();
        var error = ValidateStudentId(studentId);
        if (error is not null)
            return error;

        var now = DateTime.UtcNow;

        await using var tx = await db.Database.BeginTransactionAsync();

        var cls = (await db.Classes
                .FromSql($"""SELECT * FROM "Classes" WHERE "Id" = {classId} FOR UPDATE""")
                .ToListAsync())
            .SingleOrDefault();
        if (cls is null)
            return Problem("Class not found.", statusCode: 404);

        await db.Holds
            .Where(h => h.ClassId == classId && h.Status == HoldStatus.HELD && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.EXPIRED));

        var active = db.Holds.Where(h => h.ClassId == classId &&
            (h.Status == HoldStatus.CONFIRMED || (h.Status == HoldStatus.HELD && h.ExpiresAt > now)));

        if (await active.AnyAsync(h => h.StudentId == studentId))
            return Problem("You already have an active application or admission for this class.",
                statusCode: 409, title: "Already applied");

        if (await active.CountAsync() >= cls.TotalSeats)
            return Problem("Sorry, there are no seats available in this class right now.",
                statusCode: 409, title: "Class full");

        var hold = new Hold
        {
            ClassId = classId,
            StudentId = studentId,
            Status = HoldStatus.HELD,
            CreatedAt = now,
            ExpiresAt = TruncateToSeconds(now + settings.Duration),
        };
        db.Holds.Add(hold);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return Created($"/holds/{hold.Id}", new HoldCreatedResponse(
            hold.Id, hold.ClassId, hold.StudentId, hold.Status.ToString(), hold.ExpiresAt));
    }

    [HttpGet("/holds/{holdId:guid}")]
    public async Task<IActionResult> Get(Guid holdId)
    {
        var hold = await db.Holds
            .Include(h => h.Class)
            .FirstOrDefaultAsync(h => h.Id == holdId);

        if (hold is null)
            return Problem("Application not found.", statusCode: 404);

        return Ok(ToResponse(hold, DateTime.UtcNow));
    }

    [HttpGet("/me/holds")]
    public async Task<IActionResult> MyHolds()
    {
        var studentId = Request.Headers[StudentIdHeader].ToString().Trim();
        var error = ValidateStudentId(studentId);
        if (error is not null)
            return error;

        var holds = await db.Holds
            .Include(h => h.Class)
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        var now = DateTime.UtcNow;
        return Ok(holds.Select(h => ToResponse(h, now)));
    }

    [HttpPost("/holds/{holdId:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid holdId)
    {
        var studentId = Request.Headers[StudentIdHeader].ToString().Trim();
        var error = ValidateStudentId(studentId);
        if (error is not null)
            return error;

        var now = DateTime.UtcNow;

        await db.Holds
            .Where(h => h.Id == holdId
                     && h.StudentId == studentId
                     && h.Status == HoldStatus.HELD
                     && h.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.CONFIRMED));

        var hold = await db.Holds.AsNoTracking().FirstOrDefaultAsync(h => h.Id == holdId);

        if (hold is null)
            return Problem("Application not found.", statusCode: 404);

        if (hold.StudentId != studentId)
            return Problem("This application belongs to a different student.", statusCode: 403);

        var status = CurrentStatus(hold, now);

        if (status == HoldStatus.CONFIRMED)
            return Ok(new ConfirmResponse(hold.Id, status.ToString()));

        if (status == HoldStatus.EXPIRED)
            return Problem("The time to pay for this application has expired. Please apply again.",
                statusCode: 409, title: "Application expired");

        return Problem("This application was cancelled and can no longer be paid for.",
            statusCode: 409, title: "Application cancelled");
    }

    [HttpDelete("/holds/{holdId:guid}")]
    public async Task<IActionResult> Cancel(Guid holdId)
    {
        var studentId = Request.Headers[StudentIdHeader].ToString().Trim();
        var error = ValidateStudentId(studentId);
        if (error is not null)
            return error;

        var now = DateTime.UtcNow;

        await db.Holds
            .Where(h => h.Id == holdId
                     && h.StudentId == studentId
                     && h.Status == HoldStatus.HELD
                     && h.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.CANCELLED));

        var hold = await db.Holds.AsNoTracking().FirstOrDefaultAsync(h => h.Id == holdId);

        if (hold is null)
            return Problem("Application not found.", statusCode: 404);

        if (hold.StudentId != studentId)
            return Problem("This application belongs to a different student.", statusCode: 403);

        var status = CurrentStatus(hold, now);

        if (status == HoldStatus.CANCELLED)
            return NoContent();

        if (status == HoldStatus.CONFIRMED)
            return Problem("This admission is already confirmed and cannot be cancelled.",
                statusCode: 409, title: "Already confirmed");

        return Problem("This application has already expired.",
            statusCode: 409, title: "Application expired");
    }

    private static HoldResponse ToResponse(Hold hold, DateTime now) =>
        new(hold.Id, hold.ClassId, hold.Class.Name, hold.StudentId,
            CurrentStatus(hold, now).ToString(), hold.ExpiresAt);

    private static HoldStatus CurrentStatus(Hold hold, DateTime now)
    {
        if (hold.Status == HoldStatus.HELD && hold.ExpiresAt <= now)
            return HoldStatus.EXPIRED;

        return hold.Status;
    }

    private IActionResult? ValidateStudentId(string studentId)
    {
        if (studentId.Length == 0)
            return Problem("Please enter the student's birth registration number.",
                statusCode: 400, title: "Missing student ID");

        if (studentId.Length != StudentIdLength || !studentId.All(char.IsAsciiDigit))
            return Problem("Birth registration number must be exactly 17 digits.",
                statusCode: 400, title: "Invalid student ID");

        return null;
    }

    private static DateTime TruncateToSeconds(DateTime t) =>
        new(t.Ticks - t.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);
}
