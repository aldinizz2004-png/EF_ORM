
namespace FootballTrainingManagement.Entities;

public class Player
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public ICollection<TrainingSession> TrainingSessions { get; set; }
        = new List<TrainingSession>();
}
