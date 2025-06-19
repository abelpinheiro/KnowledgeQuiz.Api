using KnowledgeQuiz.Api.Domain;
using KnowledgeQuiz.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeQuiz.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
}