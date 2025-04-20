using KnowledgeQuiz.Api.Domain.Entities;
using KnowledgeQuiz.Api.Domain.Enums;
using KnowledgeQuiz.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeQuiz.Api.Infrastructure.Seeding;

public class UserSeeder
{
    private static readonly User DefaultAdmin= new User
    {
        Email = "admin@email.com",
        Name = "Admin",
        Password = BCrypt.Net.BCrypt.HashPassword("admin")
    };
    
    public static async Task EnsureAdminUserExistsAsync(AppDbContext context)
    {
        var hasUser = await context.Users.AnyAsync(x => x.Email == DefaultAdmin.Email);

        if (!hasUser)
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(x => x.Name.ToLower() == SystemRoles.Admin.ToString().ToLower());
            DefaultAdmin.RoleId = adminRole!.Id;
            DefaultAdmin.Role = adminRole;
            
            await context.Users.AddAsync(DefaultAdmin);
            await context.SaveChangesAsync();
        }
    }
}