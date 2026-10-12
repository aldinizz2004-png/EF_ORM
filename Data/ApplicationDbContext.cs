
using FootballTrainingManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballTrainingManagement.Data;

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

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Coach>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<TrainingSession>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(t => t.Title)
                .IsUnique();

            entity.HasOne(t => t.Coach)
                .WithMany(c => c.TrainingSessions)
                .HasForeignKey(t => t.CoachId)
                .IsRequired();

            entity.HasMany(t => t.Players)
                .WithMany(p => p.TrainingSessions);
        });
    }
}
