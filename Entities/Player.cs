using System.ComponentModel.DataAnnotations;

namespace FootballTrainingManagement.Entities;

public class Player
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    // Many-to-Many: Player <-> TrainingSession
    public ICollection<TrainingSession> TrainingSessions { get; set; }
        = new List<TrainingSession>();
}
