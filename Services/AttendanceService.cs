using FootballTrainingManagement.Data;
using FootballTrainingManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballTrainingManagement.Services;

// Handles the Many-to-Many relationship Player <-> TrainingSession.
// We never touch the join table directly: EF Core manages
// PlayerTrainingSession for us when we add/remove from the navigation.
public class AttendanceService
{
    private readonly ApplicationDbContext _db;

    public AttendanceService(ApplicationDbContext db)
    {
        _db = db;
    }

    // Assign an existing player to an existing training session
    public async Task<OperationResult> AssignPlayerAsync(int sessionId, int playerId)
    {
        var session = await _db.TrainingSessions
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == sessionId);
        if (session is null)
            return OperationResult.Fail($"Training session with ID {sessionId} was not found.");

        var player = await _db.Players.FindAsync(playerId);
        if (player is null)
            return OperationResult.Fail($"Player with ID {playerId} was not found.");

        if (session.Players.Any(p => p.Id == playerId))
            return OperationResult.Fail($"Player '{player.Name}' is already assigned to '{session.Title}'.");

        session.Players.Add(player);
        await _db.SaveChangesAsync();

        return OperationResult.Ok($"Player '{player.Name}' assigned to '{session.Title}'.");
    }

    // Remove a player from a training session
    public async Task<OperationResult> RemovePlayerAsync(int sessionId, int playerId)
    {
        var session = await _db.TrainingSessions
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == sessionId);
        if (session is null)
            return OperationResult.Fail($"Training session with ID {sessionId} was not found.");

        var playerExists = await _db.Players.AnyAsync(p => p.Id == playerId);
        if (!playerExists)
            return OperationResult.Fail($"Player with ID {playerId} was not found.");

        var player = session.Players.FirstOrDefault(p => p.Id == playerId);
        if (player is null)
            return OperationResult.Fail($"Player {playerId} is not assigned to '{session.Title}'.");

        session.Players.Remove(player);
        await _db.SaveChangesAsync();

        return OperationResult.Ok($"Player '{player.Name}' removed from '{session.Title}'.");
    }

    // Display all players participating in a specific training session
    // Returns null when the session ID does not exist.
    public async Task<List<Player>?> GetPlayersInSessionAsync(int sessionId)
    {
        var sessionExists = await _db.TrainingSessions.AnyAsync(t => t.Id == sessionId);
        if (!sessionExists)
            return null;

        return await _db.Players
            .AsNoTracking()
            .Where(p => p.TrainingSessions.Any(t => t.Id == sessionId))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    // Display all training sessions attended by a specific player
    // Returns null when the player ID does not exist.
    public async Task<List<TrainingSession>?> GetSessionsForPlayerAsync(int playerId)
    {
        var playerExists = await _db.Players.AnyAsync(p => p.Id == playerId);
        if (!playerExists)
            return null;

        return await _db.TrainingSessions
            .AsNoTracking()
            .Include(t => t.Coach)
            .Include(t => t.Players)
            .Where(t => t.Players.Any(p => p.Id == playerId))
            .OrderBy(t => t.Date)
            .ToListAsync();
    }
}
