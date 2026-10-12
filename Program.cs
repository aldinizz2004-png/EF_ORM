
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

Console.WriteLine(
    $"DbContext configured: {dbContext.Database.ProviderName}");

Console.WriteLine("EF Core configuration completed.");
