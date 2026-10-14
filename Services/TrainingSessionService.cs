using FootballTrainingManagement.Data;
using FootballTrainingManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballTrainingManagement.Services;

public class TrainingSessionService
{
    private readonly ApplicationDbContext _db;

    public TrainingSessionService(ApplicationDbContext db)
    {
        _db = db;
    }

    // Create a new training session and assign it to an existing coach
    public async Task<OperationResult> AddAsync(string title, DateTime date, int coachId)
    {
        title = title.Trim();

        var coachExists = await _db.Coaches.AnyAsync(c => c.Id == coachId);
        if (!coachExists)
            return OperationResult.Fail($"Coach with ID {coachId} was not found.");

        if (await TitleExistsAsync(title))
            return OperationResult.Fail($"A training session with the title '{title}' already exists.");

        var session = new TrainingSession
        {
            Title = title,
            Date = date,
            CoachId = coachId
        };

        await _db.TrainingSessions.AddAsync(session);

        return await SaveAsync(session, () => $"Training session created with ID {session.Id}.");
    }

    // Display all training sessions (with the coach and player count)
    public async Task<List<TrainingSession>> GetAllAsync()
    {
        return await _db.TrainingSessions
            .AsNoTracking()
            .Include(t => t.Coach)
            .Include(t => t.Players)
            .OrderBy(t => t.Date)
            .ToListAsync();
    }

    // Find a training session by ID (returns null when the ID does not exist)
    public async Task<TrainingSession?> GetByIdAsync(int id)
    {
        return await _db.TrainingSessions
            .AsNoTracking()
            .Include(t => t.Coach)
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    // Update a session's title, date, or assigned coach.
    // Pass null for any value that should stay the same.
    public async Task<OperationResult> UpdateAsync(
        int id, string? newTitle, DateTime? newDate, int? newCoachId)
    {
        var session = await _db.TrainingSessions.FindAsync(id);
        if (session is null)
            return OperationResult.Fail($"Training session with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(newTitle))
        {
            newTitle = newTitle.Trim();

            // Titles must stay unique (ignore the session we are editing)
            if (await TitleExistsAsync(newTitle, excludeId: id))
                return OperationResult.Fail($"A training session with the title '{newTitle}' already exists.");

            session.Title = newTitle;
        }

        if (newDate.HasValue)
            session.Date = newDate.Value;

        if (newCoachId.HasValue)
        {
            var coachExists = await _db.Coaches.AnyAsync(c => c.Id == newCoachId.Value);
            if (!coachExists)
                return OperationResult.Fail($"Coach with ID {newCoachId.Value} was not found.");

            session.CoachId = newCoachId.Value;
        }

        return await SaveAsync(session, () => $"Training session {id} updated successfully.");
    }

    // Delete a training session
    public async Task<OperationResult> DeleteAsync(int id)
    {
        var session = await _db.TrainingSessions.FindAsync(id);
        if (session is null)
            return OperationResult.Fail($"Training session with ID {id} was not found.");

        _db.TrainingSessions.Remove(session);
        await _db.SaveChangesAsync();

        return OperationResult.Ok($"Training session {id} deleted successfully.");
    }

    private async Task<bool> TitleExistsAsync(string title, int? excludeId = null)
    {
        return await _db.TrainingSessions
            .AnyAsync(t => t.Title == title && (excludeId == null || t.Id != excludeId));
    }

    // Saves changes and turns a unique-index violation into a friendly message.
    // The AnyAsync check above catches duplicates first; this is a safety net
    // in case another user inserted the same title at the same moment.
    private async Task<OperationResult> SaveAsync(TrainingSession session, Func<string> successMessage)
    {
        try
        {
            await _db.SaveChangesAsync();
            return OperationResult.Ok(successMessage());
        }
        catch (DbUpdateException)
        {
            var title = session.Title;
            var entry = _db.Entry(session);

            // Undo the failed change so the DbContext stays usable
            if (entry.State == EntityState.Added)
                entry.State = EntityState.Detached;
            else
                await entry.ReloadAsync();

            return OperationResult.Fail(
                $"Could not save: the title '{title}' is already used by another training session.");
        }
    }
}
