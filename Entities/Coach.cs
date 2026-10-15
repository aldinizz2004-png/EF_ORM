using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FootballTrainingManagement.Entities;

public class Coach
{
    // Standard starting monthly salary for every new coach
    public const decimal DefaultMonthlySalary = 1500m;

    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Monthly salary in USD -> stored as decimal(18,2) in SQL Server.
    // The database default (1500) is configured with Fluent API in ApplicationDbContext.
    [Precision(18, 2)]
    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Salary { get; set; } = DefaultMonthlySalary;

    // One-to-Many: one Coach leads many TrainingSessions
    public ICollection<TrainingSession> TrainingSessions { get; set; }
        = new List<TrainingSession>();
}
