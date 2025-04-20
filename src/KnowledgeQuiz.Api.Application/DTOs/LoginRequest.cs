using System.ComponentModel.DataAnnotations;

namespace KnowledgeQuiz.Api.Application.DTOs;

public class LoginRequest
{
    [Required, EmailAddress] 
    public string Email { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
}