using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FootballTrainingManagement.Entities;

// Unique constraint on Title
[Index(nameof(Title), IsUnique = true)]
public class TrainingSession
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    // Foreign key to Coach (required because int is non-nullable)
    public int CoachId { get; set; }

    [ForeignKey(nameof(CoachId))]
    public Coach Coach { get; set; } = null!;

    // Many-to-Many: TrainingSession <-> Player
    public ICollection<Player> Players { get; set; }
        = new List<Player>();
}
