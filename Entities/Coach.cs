
namespace FootballTrainingManagement.Entities;

public class Coach
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<TrainingSession> TrainingSessions { get; set; }
        = new List<TrainingSession>();
}
