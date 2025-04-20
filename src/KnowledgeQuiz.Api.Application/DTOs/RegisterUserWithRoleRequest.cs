using System.ComponentModel.DataAnnotations;

namespace KnowledgeQuiz.Api.Application.DTOs;

public class RegisterUserWithRoleRequest : RegisterUserRequest
{
    [Required]
    public string Role { get; set; } = null!;
}