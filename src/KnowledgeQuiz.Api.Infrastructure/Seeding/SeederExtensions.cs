using KnowledgeQuiz.Api.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KnowledgeQuiz.Api.Infrastructure.Seeding;

public static class SeederExtensions
{
    /// <summary>
    /// Routine to seed essential data for the application, guaranteeing that
    /// during initialization the database has standard content data
    /// and preventing runtime failures due to the absence of them.
    /// </summary>
    /// <param name="app">Instance of an ASP.NET application</param>
    /// <param name="logger"></param>
    public static async Task SeedAsync(this IHost app, ILogger logger)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var db = services.GetRequiredService<AppDbContext>();
            var config = services.GetRequiredService<IConfiguration>();

            logger.Information("Starting role seeding...");
            await RoleSeeder.EnsureSystemRolesExistAsync(db, logger);

            logger.Information("Starting Admin User Seeding...");
            await UserSeeder.EnsureAdminUserExistsAsync(db, config, logger);

            logger.Information("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error during database seeding.");
            throw;
        }
    }
}