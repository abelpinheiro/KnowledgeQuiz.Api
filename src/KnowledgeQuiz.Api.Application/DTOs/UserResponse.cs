namespace KnowledgeQuiz.Api.Application.DTOs;

public class UserResponse
{
    public int Id { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Role { get; set; }
}