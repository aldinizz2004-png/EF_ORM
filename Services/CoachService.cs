using FootballTrainingManagement.Data;
using FootballTrainingManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballTrainingManagement.Services;

public class CoachService
{
    private readonly ApplicationDbContext _db;

    public CoachService(ApplicationDbContext db)
    {
        _db = db;
    }

    // Add a new coach
    public async Task<Coach> AddAsync(string name)
    {
        var coach = new Coach { Name = name.Trim() };

        await _db.Coaches.AddAsync(coach);
        await _db.SaveChangesAsync();

        return coach;
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

    // Update a coach's information
    public async Task<OperationResult> UpdateAsync(int id, string name)
    {
        var coach = await _db.Coaches.FindAsync(id);
        if (coach is null)
            return OperationResult.Fail($"Coach with ID {id} was not found.");

        coach.Name = name.Trim();

        await _db.SaveChangesAsync();
        return OperationResult.Ok($"Coach {id} updated successfully.");
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
}
