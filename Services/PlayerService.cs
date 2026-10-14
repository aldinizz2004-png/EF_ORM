using FootballTrainingManagement.Data;
using FootballTrainingManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballTrainingManagement.Services;

public class PlayerService
{
    private readonly ApplicationDbContext _db;

    public PlayerService(ApplicationDbContext db)
    {
        _db = db;
    }

    // Add a new player
    public async Task<Player> AddAsync(string name, int age)
    {
        var player = new Player { Name = name.Trim(), Age = age };

        await _db.Players.AddAsync(player);
        await _db.SaveChangesAsync();

        return player;
    }

    // Display all players
    public async Task<List<Player>> GetAllAsync()
    {
        return await _db.Players
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    // Find a player by ID (returns null when the ID does not exist)
    public async Task<Player?> GetByIdAsync(int id)
    {
        return await _db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Update a player's information
    public async Task<OperationResult> UpdateAsync(int id, string name, int age)
    {
        var player = await _db.Players.FindAsync(id);
        if (player is null)
            return OperationResult.Fail($"Player with ID {id} was not found.");

        player.Name = name.Trim();
        player.Age = age;

        await _db.SaveChangesAsync();
        return OperationResult.Ok($"Player {id} updated successfully.");
    }

    // Delete a player (EF Core also removes the player's rows
    // from the PlayerTrainingSession join table - cascade delete)
    public async Task<OperationResult> DeleteAsync(int id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player is null)
            return OperationResult.Fail($"Player with ID {id} was not found.");

        _db.Players.Remove(player);
        await _db.SaveChangesAsync();

        return OperationResult.Ok($"Player {id} deleted successfully.");
    }
}
