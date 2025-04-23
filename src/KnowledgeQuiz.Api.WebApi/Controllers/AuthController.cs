using KnowledgeQuiz.Api.Application.Contracts;
using KnowledgeQuiz.Api.Application.DTOs;
using KnowledgeQuiz.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeQuiz.Api.WebApi.Controllers;

/// <summary>
/// Controller for handling authentication endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _repository;

    public AuthController(IUserRepository repository)
    {
        _repository = repository;
    }
    
    /// <summary>
    /// Login in the system
    /// </summary>
    /// <param name="loginRequest"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<string>>> LoginAsync(LoginRequest loginRequest)
    {
        var result = await _repository.LoginUserAsync(loginRequest);
        if (!result.Success)
        {
            var message = result.Reason switch
            {
                LoginFailureReason.InvalidCredentials => "Email or password is incorrect",
                _=> "Login failed"
            };
            
            return Unauthorized(ApiResponse<string>.Fail(message));
        }
        
        return Ok(ApiResponse<string>.SuccessResponse(result.Token!, "Login successful"));
    }
    
    /// <summary>
    /// Creates an account with default role of player
    /// </summary>
    /// <param name="registerUserRequest"></param>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> RegisterAsync(RegisterUserRequest registerUserRequest)
    {
        var result = await _repository.RegisterUserAsync(registerUserRequest, "player");

        if (!result.Success)
        {
            var message = result.FailureReason switch
            {
                RegisterFailureReason.UserAlreadyExists => "An account with this email already exists.",
                _ => "Registration failed."
            };
            
            return BadRequest(ApiResponse<string>.Fail(message));
        }
        
        return Ok(ApiResponse<string>.SuccessResponse(null!, "Registration successful."));
    }
}