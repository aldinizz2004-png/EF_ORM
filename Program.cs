using FootballTrainingManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "The database connection string was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var dbContext = scope.ServiceProvider
    .GetRequiredService<ApplicationDbContext>();

Console.WriteLine($"Provider: {dbContext.Database.ProviderName}");

// 1) Verify the database connection
if (!dbContext.Database.CanConnect())
{
    Console.WriteLine("Could not connect to the database. " +
                      "Run 'dotnet ef database update' and check the connection string.");
    return;
}
Console.WriteLine("Database connection successful.");

// 2) Verify migrations
var applied = dbContext.Database.GetAppliedMigrations().ToList();
var pending = dbContext.Database.GetPendingMigrations().ToList();

Console.WriteLine($"Applied migrations ({applied.Count}):");
applied.ForEach(m => Console.WriteLine($"  - {m}"));

if (pending.Count > 0)
{
    Console.WriteLine($"Pending migrations ({pending.Count}):");
    pending.ForEach(m => Console.WriteLine($"  - {m}"));
}
else
{
    Console.WriteLine("No pending migrations. Database schema is up to date.");
}

// 3) Verify the schema: query every table
Console.WriteLine($"Players: {dbContext.Players.Count()}");
Console.WriteLine($"Coaches: {dbContext.Coaches.Count()}");
Console.WriteLine($"TrainingSessions: {dbContext.TrainingSessions.Count()}");

Console.WriteLine("EF Core configuration verified.");
