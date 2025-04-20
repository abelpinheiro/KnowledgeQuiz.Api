using KnowledgeQuiz.Api.Application.DTOs;

namespace KnowledgeQuiz.Api.Application.Contracts;

public interface IUserRepository
{
    Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest registerUserRequest, string userRole);
    Task<LoginResponse> LoginUserAsync(LoginRequest loginRequest);
    Task<List<UserResponse>> GetUsersAsync();
    Task<RegisterUserResponse> AssignRoleToUserAsync(int userId, AssignRoleRequest assignRoleRequest);
}