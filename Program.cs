using FootballTrainingManagement.Data;
using FootballTrainingManagement.Entities;
using FootballTrainingManagement.DTOs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration: Use connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add minimal API services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure database created (quick dev mode)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Creates DB if not exists according to model (no migrations required)
    db.Database.EnsureCreated();

    // Optional: seed small data if empty
    if (!db.Coaches.Any())
    {
        db.Coaches.AddRange(
            new Coach { Name = "Coach A" },
            new Coach { Name = "Coach B" });
        db.Players.AddRange(
            new Player { Name = "Player One", Age = 20 },
            new Player { Name = "Player Two", Age = 22 });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok("Football Training Management API"));

/* -------------------- Players CRUD -------------------- */

// GET all players
app.MapGet("/players", async (ApplicationDbContext db) =>
{
    var players = await db.Players
        .Include(p => p.TrainingSessions)
        .ToListAsync();
    var dto = players.Select(p => new PlayerDto
    {
        Id = p.Id,
        Name = p.Name,
        Age = p.Age,
        TrainingSessionIds = p.TrainingSessions.Select(ts => ts.Id).ToList()
    });
    return Results.Ok(dto);
});

// GET player by id
app.MapGet("/players/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var p = await db.Players
        .Include(x => x.TrainingSessions)
        .FirstOrDefaultAsync(x => x.Id == id);

    return p is null
        ? Results.NotFound()
        : Results.Ok(new PlayerDto
        {
            Id = p.Id,
            Name = p.Name,
            Age = p.Age,
            TrainingSessionIds = p.TrainingSessions.Select(ts => ts.Id).ToList()
        });
});

// POST create player
app.MapPost("/players", async (CreatePlayerDto input, ApplicationDbContext db) =>
{
    var p = new Player { Name = input.Name, Age = input.Age };
    db.Players.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/players/{p.Id}", new PlayerDto { Id = p.Id, Name = p.Name, Age = p.Age, TrainingSessionIds = new List<int>() });
});

// PUT update player
app.MapPut("/players/{id:int}", async (int id, UpdatePlayerDto input, ApplicationDbContext db) =>
{
    var p = await db.Players.FindAsync(id);
    if (p is null) return Results.NotFound();
    p.Name = input.Name;
    p.Age = input.Age;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE player
app.MapDelete("/players/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var p = await db.Players.FindAsync(id);
    if (p is null) return Results.NotFound();
    db.Players.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

/* -------------------- Coaches CRUD -------------------- */

// GET all coaches
app.MapGet("/coaches", async (ApplicationDbContext db) =>
{
    var coaches = await db.Coaches.Include(c => c.TrainingSessions).ToListAsync();
    var dto = coaches.Select(c => new CoachDto
    {
        Id = c.Id,
        Name = c.Name,
        TrainingSessionIds = c.TrainingSessions.Select(ts => ts.Id).ToList()
    });
    return Results.Ok(dto);
});

app.MapGet("/coaches/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var c = await db.Coaches.Include(x => x.TrainingSessions).FirstOrDefaultAsync(x => x.Id == id);
    if (c is null) return Results.NotFound();
    return Results.Ok(new CoachDto { Id = c.Id, Name = c.Name, TrainingSessionIds = c.TrainingSessions.Select(ts => ts.Id).ToList() });
});

app.MapPost("/coaches", async (CreateCoachDto input, ApplicationDbContext db) =>
{
    var c = new Coach { Name = input.Name };
    db.Coaches.Add(c);
    await db.SaveChangesAsync();
    return Results.Created($"/coaches/{c.Id}", new CoachDto { Id = c.Id, Name = c.Name, TrainingSessionIds = new List<int>() });
});

app.MapPut("/coaches/{id:int}", async (int id, UpdateCoachDto input, ApplicationDbContext db) =>
{
    var c = await db.Coaches.FindAsync(id);
    if (c is null) return Results.NotFound();
    c.Name = input.Name;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/coaches/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var c = await db.Coaches.FindAsync(id);
    if (c is null) return Results.NotFound();
    db.Coaches.Remove(c);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

/* -------------------- TrainingSessions CRUD -------------------- */

// GET all sessions (include coach and players)
app.MapGet("/trainingsessions", async (ApplicationDbContext db) =>
{
    var sessions = await db.TrainingSessions
        .Include(ts => ts.Coach)
        .Include(ts => ts.Players)
        .ToListAsync();

    var dto = sessions.Select(ts => new TrainingSessionDto
    {
        Id = ts.Id,
        Title = ts.Title,
        Date = ts.Date,
        CoachId = ts.CoachId,
        CoachName = ts.Coach?.Name ?? string.Empty,
        PlayerIds = ts.Players.Select(p => p.Id).ToList()
    });

    return Results.Ok(dto);
});

// GET one session
app.MapGet("/trainingsessions/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var ts = await db.TrainingSessions
        .Include(t => t.Coach)
        .Include(t => t.Players)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (ts is null) return Results.NotFound();

    var dto = new TrainingSessionDto
    {
        Id = ts.Id,
        Title = ts.Title,
        Date = ts.Date,
        CoachId = ts.CoachId,
        CoachName = ts.Coach?.Name ?? string.Empty,
        PlayerIds = ts.Players.Select(p => p.Id).ToList()
    };
    return Results.Ok(dto);
});

// POST create session (optionally with player ids)
app.MapPost("/trainingsessions", async (CreateTrainingSessionDto input, ApplicationDbContext db) =>
{
    // Validate coach exists
    var coach = await db.Coaches.FindAsync(input.CoachId);
    if (coach is null) return Results.BadRequest($"Coach with id {input.CoachId} not found.");

    // Fetch players if provided
    var players = new List<Player>();
    if (input.PlayerIds?.Any() == true)
    {
        players = await db.Players.Where(p => input.PlayerIds.Contains(p.Id)).ToListAsync();
        // optional: check for missing ids
        var missing = input.PlayerIds.Except(players.Select(p => p.Id)).ToList();
        if (missing.Any()) return Results.BadRequest($"Player ids not found: {string.Join(',', missing)}");
    }

    var ts = new TrainingSession
    {
        Title = input.Title,
        Date = input.Date,
        CoachId = input.CoachId,
        Players = players
    };

    db.TrainingSessions.Add(ts);
    try
    {
        await db.SaveChangesAsync();
    }
    catch (DbUpdateException ex)
    {
        return Results.Conflict(ex.Message);
    }

    var dto = new TrainingSessionDto
    {
        Id = ts.Id,
        Title = ts.Title,
        Date = ts.Date,
        CoachId = ts.CoachId,
        CoachName = coach.Name,
        PlayerIds = ts.Players.Select(p => p.Id).ToList()
    };
    return Results.Created($"/trainingsessions/{ts.Id}", dto);
});

// PUT update session (title, date, coach; does not change players list)
app.MapPut("/trainingsessions/{id:int}", async (int id, UpdateTrainingSessionDto input, ApplicationDbContext db) =>
{
    var ts = await db.TrainingSessions.FindAsync(id);
    if (ts is null) return Results.NotFound();

    // validate coach
    var coach = await db.Coaches.FindAsync(input.CoachId);
    if (coach is null) return Results.BadRequest($"Coach with id {input.CoachId} not found.");

    ts.Title = input.Title;
    ts.Date = input.Date;
    ts.CoachId = input.CoachId;

    try
    {
        await db.SaveChangesAsync();
    }
    catch (DbUpdateException ex)
    {
        return Results.Conflict(ex.Message);
    }

    return Results.NoContent();
});

// DELETE session
app.MapDelete("/trainingsessions/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var ts = await db.TrainingSessions.FindAsync(id);
    if (ts is null) return Results.NotFound();
    db.TrainingSessions.Remove(ts);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

/* -------------------- TrainingSession -> Players (manage membership) -------------------- */

// Add player(s) to a session
app.MapPost("/trainingsessions/{id:int}/players", async (int id, AddPlayersToSessionDto input, ApplicationDbContext db) =>
{
    var ts = await db.TrainingSessions.Include(t => t.Players).FirstOrDefaultAsync(t => t.Id == id);
    if (ts is null) return Results.NotFound();

    var players = await db.Players.Where(p => input.PlayerIds.Contains(p.Id)).ToListAsync();
    var missing = input.PlayerIds.Except(players.Select(p => p.Id)).ToList();
    if (missing.Any()) return Results.BadRequest($"Player ids not found: {string.Join(',', missing)}");

    foreach (var p in players)
    {
        if (!ts.Players.Any(x => x.Id == p.Id))
            ts.Players.Add(p);
    }

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Remove a player from a session
app.MapDelete("/trainingsessions/{id:int}/players/{playerId:int}", async (int id, int playerId, ApplicationDbContext db) =>
{
    var ts = await db.TrainingSessions.Include(t => t.Players).FirstOrDefaultAsync(t => t.Id == id);
    if (ts is null) return Results.NotFound();

    var player = ts.Players.FirstOrDefault(p => p.Id == playerId);
    if (player is null) return Results.NotFound($"Player {playerId} is not part of TrainingSession {id}");

    ts.Players.Remove(player);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();