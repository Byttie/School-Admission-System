using Microsoft.EntityFrameworkCore;

namespace AdmissionSystem.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Hold> Holds => Set<Hold>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<SchoolClass>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
        });

        b.Entity<Hold>(e =>
        {
            e.Property(h => h.StudentId).HasMaxLength(17).IsRequired();
            e.Property(h => h.Status).HasConversion<string>().HasMaxLength(16);
            e.HasOne(h => h.Class).WithMany(c => c.Holds)
             .HasForeignKey(h => h.ClassId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(h => new { h.ClassId, h.Status, h.ExpiresAt });
            e.HasIndex(h => h.StudentId);
        });
    }
}
