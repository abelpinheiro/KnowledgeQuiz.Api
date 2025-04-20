using KnowledgeQuiz.Api.Domain.Entities;
using KnowledgeQuiz.Api.Domain.Enums;
using KnowledgeQuiz.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KnowledgeQuiz.Api.Infrastructure.Seeding;

public class UserSeeder
{
    public static async Task EnsureAdminUserExistsAsync(AppDbContext context, IConfiguration config)
    {
        var adminEmail = config["Seed:AdminEmail"] ?? "admin@email.com";
        var adminName = config["Seed:AdminName"] ?? "Admin";
        var adminPassword = config["Seed:AdminPassword"];
        
        if(string.IsNullOrEmpty(adminPassword))
            throw new Exception("Admin password not found in configuration");
        
        var hasUser = await context.Users.AnyAsync(x => x.Email == adminEmail);

        if (!hasUser)
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(x => x.Name.ToLower() == SystemRoles.Admin.ToString().ToLower());

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
        }
    }
}