using KnowledgeQuiz.Api.Domain.Entities;
using KnowledgeQuiz.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowledgeQuiz.Api.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder
            .ToTable("roles")
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.HasData(
            new Role{ Id = 1, Name = "admin"},
            new Role{ Id = 2, Name = "creator"},
            new Role{ Id = 3, Name = "analytics"},
            new Role{ Id = 4, Name = "player" }
        );
    }
}