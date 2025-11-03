using Microsoft.EntityFrameworkCore;
using RequirementAgent.Api.Models;
using RequirementAgent.Api.Models.Enums;

namespace RequirementAgent.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PermitType> PermitTypes => Set<PermitType>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Upload> Uploads => Set<Upload>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Question>()
            .HasIndex(q => new { q.PermitTypeId, q.Order })
            .IsUnique();

        modelBuilder.Entity<Question>()
            .Property(q => q.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<Question>()
            .Property(q => q.OptionsJson)
            .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<Submission>()
            .Property(s => s.AnswersJson)
            .HasColumnType("nvarchar(max)");

        var adminId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = adminId,
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMeNow!"),
                Role = UserRole.Admin
            },
            new User
            {
                Id = clientId,
                Email = "client@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("ClientPass123!"),
                Role = UserRole.Client
            }
        );
    }
}
