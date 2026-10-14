using FootballTrainingManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballTrainingManagement.Data;

// All configuration (keys, required, max length, unique index, foreign key)
// is done with Data Annotations inside the entity classes.
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
}
