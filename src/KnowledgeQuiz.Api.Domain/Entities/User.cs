namespace KnowledgeQuiz.Api.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public bool IsAnonymous { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public int? RoleId { get; set; }
    public Role? Role { get; set; }
}