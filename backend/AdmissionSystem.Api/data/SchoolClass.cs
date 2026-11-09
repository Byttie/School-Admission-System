namespace AdmissionSystem.Api.Data;

public class SchoolClass
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public int TotalSeats { get; set; }
}