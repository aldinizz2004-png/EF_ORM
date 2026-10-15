using FootballTrainingManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballTrainingManagement.Data;

// Most configuration (keys, required, max length, unique index, foreign key,
// decimal precision) is done with Data Annotations inside the entity classes.
// Fluent API is used only where no annotation exists (database default values).
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();

    public DbSet<Coach> Coaches => Set<Coach>();

    public DbSet<TrainingSession> TrainingSessions
        => Set<TrainingSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Every coach starts with a monthly salary of 1500.
        // The migration uses this default to fill the new column for existing coaches,
        // and SQL Server applies it to any future row inserted without a salary.
        modelBuilder.Entity<Coach>()
            .Property(c => c.Salary)
            .HasDefaultValue(Coach.DefaultMonthlySalary);
    }
}
