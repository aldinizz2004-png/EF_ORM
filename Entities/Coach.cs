using System.ComponentModel.DataAnnotations;

namespace FootballTrainingManagement.Entities;

public class Coach
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // One-to-Many: one Coach leads many TrainingSessions
    public ICollection<TrainingSession> TrainingSessions { get; set; }
        = new List<TrainingSession>();
}
