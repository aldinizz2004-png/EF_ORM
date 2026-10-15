using FootballTrainingManagement.Data;
using FootballTrainingManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballTrainingManagement.Services;

public class CoachService
{
    public const decimal MinSalary = 0.01m;
    public const decimal MaxSalary = 1_000_000m;

    private readonly ApplicationDbContext _db;

    public CoachService(ApplicationDbContext db)
    {
        _db = db;
    }

    // Add a new coach.
    // When no salary is given the coach gets the standard starting salary (1500).
    public async Task<OperationResult> AddAsync(string name, decimal? salary = null)
    {
        var finalSalary = salary ?? Coach.DefaultMonthlySalary;
        if (!IsValidSalary(finalSalary))
            return OperationResult.Fail(SalaryRangeMessage);

        var coach = new Coach
        {
            Name = name.Trim(),
            Salary = finalSalary
        };

        await _db.Coaches.AddAsync(coach);
        await _db.SaveChangesAsync();

        return OperationResult.Ok($"Coach added with ID {coach.Id} and a monthly salary of {coach.Salary:C}.");
    }

    // Display all coaches (with their sessions so we can show a count)
    public async Task<List<Coach>> GetAllAsync()
    {
        return await _db.Coaches
            .AsNoTracking()
            .Include(c => c.TrainingSessions)
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    // Find a coach by ID (returns null when the ID does not exist)
    public async Task<Coach?> GetByIdAsync(int id)
    {
        return await _db.Coaches
            .AsNoTracking()
            .Include(c => c.TrainingSessions)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _db.Coaches.AnyAsync(c => c.Id == id);
    }

    // How many sessions will be removed if this coach is deleted
    public async Task<int> CountSessionsAsync(int id)
    {
        return await _db.TrainingSessions.CountAsync(t => t.CoachId == id);
    }

    // Update a coach's full profile (name + monthly salary).
    // Loads the tracked entity, changes its values, and commits with SaveChangesAsync().
    public async Task<OperationResult> UpdateAsync(int id, string name, decimal salary)
    {
        if (!IsValidSalary(salary))
            return OperationResult.Fail(SalaryRangeMessage);

        var coach = await _db.Coaches.FindAsync(id);
        if (coach is null)
            return OperationResult.Fail($"Coach with ID {id} was not found.");

        coach.Name = name.Trim();
        coach.Salary = salary;

        await _db.SaveChangesAsync();
        return OperationResult.Ok($"Coach {id} updated: {coach.Name}, monthly salary {coach.Salary:C}.");
    }

    // Delete a coach.
    // Note: the relationship from Monday's migration uses cascade delete,
    // so deleting a coach also deletes the coach's training sessions.
    public async Task<OperationResult> DeleteAsync(int id)
    {
        var coach = await _db.Coaches.FindAsync(id);
        if (coach is null)
            return OperationResult.Fail($"Coach with ID {id} was not found.");

        _db.Coaches.Remove(coach);
        await _db.SaveChangesAsync();

        return OperationResult.Ok($"Coach {id} deleted successfully.");
    }

    private static bool IsValidSalary(decimal salary) =>
        salary >= MinSalary && salary <= MaxSalary;

    private static string SalaryRangeMessage =>
        $"Salary must be between {MinSalary:C} and {MaxSalary:C}.";
}
