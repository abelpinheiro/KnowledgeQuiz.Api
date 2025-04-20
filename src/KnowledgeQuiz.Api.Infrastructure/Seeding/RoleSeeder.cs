using KnowledgeQuiz.Api.Domain.Entities;
using KnowledgeQuiz.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeQuiz.Api.Infrastructure.Seeding;

/// <summary>
/// Provides the system with the default essential roles for the system
/// </summary>
public static class RoleSeeder
{
    // List of the default roles available to the system
    private static readonly List<Role> _defaultRoles = new()
    {
        new Role { Name = "admin" },
        new Role { Name = "creator" },
        new Role { Name = "analytics" },
        new Role { Name = "player"}
    };

    /// <summary>
    /// Check if the database has the default roles available. If not, create them.
    /// </summary>
    /// <param name="context">database context</param>
    public static async Task EnsureSystemRolesExistAsync(AppDbContext context)
    {
        foreach (var role in _defaultRoles)
        {
            var hasRole = await context.Roles.AnyAsync(x => x.Name.ToLower() == role.Name.ToLower());
            if (!hasRole)
                await context.Roles.AddAsync(role);
        }
        await context.SaveChangesAsync();
    }
}