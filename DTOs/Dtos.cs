using System.ComponentModel.DataAnnotations;

namespace FootballTrainingManagement.DTOs;

// Player DTOs
public class PlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public List<int> TrainingSessionIds { get; set; } = new();
}

public class CreatePlayerDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class UpdatePlayerDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Coach DTOs
public class CoachDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<int> TrainingSessionIds { get; set; } = new();
}

public class CreateCoachDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateCoachDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

// TrainingSession DTOs
public class TrainingSessionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CoachId { get; set; }
    public string CoachName { get; set; } = string.Empty;
    public List<int> PlayerIds { get; set; } = new();
}

public class CreateTrainingSessionDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int CoachId { get; set; }
    public List<int>? PlayerIds { get; set; }
}

public class UpdateTrainingSessionDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int CoachId { get; set; }
}

// For adding players to a session
public class AddPlayersToSessionDto
{
    [Required]
    public List<int> PlayerIds { get; set; } = new();
}