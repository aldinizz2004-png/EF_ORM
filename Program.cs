using FootballTrainingManagement.Data;
using FootballTrainingManagement.Services;
using FootballTrainingManagement.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// UTF-8 so symbols like ✔ and table borders show correctly
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Show money as US dollars ($1,500.00) regardless of the Windows region settings
var usCulture = new System.Globalization.CultureInfo("en-US");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = usCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = usCulture;
System.Globalization.CultureInfo.CurrentCulture = usCulture;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "The database connection string was not found.");

// Hide EF Core's SQL logs so they don't clutter the console menu
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

// Same DbContext + database from Monday's task
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Services that contain the CRUD logic
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<CoachService>();
builder.Services.AddScoped<TrainingSessionService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<Menus>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

// Make sure the existing database is reachable and up to date.
// We do NOT create or migrate the database here.
if (!await dbContext.Database.CanConnectAsync())
{
    ConsoleInput.Error("Could not connect to the database. " +
                       "Check the connection string and run 'dotnet ef database update'.");
    return;
}

var pending = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();
if (pending.Count > 0)
{
    ConsoleInput.Error($"The database has {pending.Count} pending migration(s). " +
                       "Run 'dotnet ef database update' first.");
    return;
}

try
{
    var menus = scope.ServiceProvider.GetRequiredService<Menus>();
    await menus.RunAsync();
}
catch (EndOfStreamException)
{
    // Input ended (no more lines to read) - just exit normally
}
catch (Exception ex)
{
    ConsoleInput.Error($"Unexpected error: {ex.Message}");
}

Console.WriteLine("Goodbye!");
