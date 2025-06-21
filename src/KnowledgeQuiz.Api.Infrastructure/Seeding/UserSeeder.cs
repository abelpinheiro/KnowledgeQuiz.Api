using KnowledgeQuiz.Api.Domain.Entities;
using KnowledgeQuiz.Api.Domain.Enums;
using KnowledgeQuiz.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace KnowledgeQuiz.Api.Infrastructure.Seeding;

public class UserSeeder
{
    public static async Task EnsureAdminUserExistsAsync(AppDbContext context, IConfiguration config, ILogger logger)
    {
        var adminEmail = config["Seed:AdminEmail"];
        var adminName = config["Seed:AdminName"];
        var adminPassword = config["Seed:AdminPassword"];

        if (string.IsNullOrEmpty(adminPassword) || string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminName))
        {
            logger.Warning("Admin credentials not fully provided. Skipping admin user creation");
            return;
        }
        
        var hasUser = await context.Users.AnyAsync(x => x.Email == adminEmail);

        if (!hasUser)
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(x => x.Name.ToLower() == SystemRoles.Admin.ToString().ToLower());

            if (adminRole == null)
            {
                logger.Error("Admin role does not exist in database");
                return;
            }
            
            var defaultAdmin = new User()
            {
                Email = adminEmail,
                Name = adminName,
                Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                RoleId = adminRole!.Id,
                Role = adminRole
            };
            
            await context.Users.AddAsync(defaultAdmin);
            await context.SaveChangesAsync();
            
            logger.Information("Default admin user created with email: {AdminEmail}", adminEmail);
        }
        else
        {
            logger.Information("Admin user already exists. Skipping creation");
        }
    }
}