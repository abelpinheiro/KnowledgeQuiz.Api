using System.ComponentModel.DataAnnotations;

namespace KnowledgeQuiz.Api.Application.DTOs;

public class RegisterUserRequest
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public string Name { get; set; } = null!;
    
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
    
    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = null!;
    
    public DateTime? DateOfBirth { get; set; }
}