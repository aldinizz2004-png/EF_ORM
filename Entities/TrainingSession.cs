
namespace FootballTrainingManagement.Entities;

public class TrainingSession
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public int CoachId { get; set; }

    public Coach Coach { get; set; } = null!;

    public ICollection<Player> Players { get; set; }
        = new List<Player>();
}
