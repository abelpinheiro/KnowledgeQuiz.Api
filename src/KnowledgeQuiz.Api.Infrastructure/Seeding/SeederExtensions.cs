using KnowledgeQuiz.Api.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KnowledgeQuiz.Api.Infrastructure.Seeding;

public static class SeederExtensions
{
    /// <summary>
    /// Routine to seed essential data for the application, guaranteeing that
    /// during initialization the database has standard content data
    /// and preventing runtime failures due to the absence of them.
    /// </summary>
    /// <param name="app">Instance of an ASP.NET application</param>
    public static async Task SeedAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<AppDbContext>();
        var config = services.GetRequiredService<IConfiguration>();
        await RoleSeeder.EnsureSystemRolesExistAsync(db);
        await UserSeeder.EnsureAdminUserExistsAsync(db, config);
    }
}