namespace AdmissionSystem.Api.Data;

public enum HoldStatus { HELD, CONFIRMED, EXPIRED, CANCELLED }

public class Hold
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassId { get; set; }
    public SchoolClass Class { get; set; } = null!;
    public string StudentId { get; set; } = "";
    public HoldStatus Status { get; set; } = HoldStatus.HELD;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}
