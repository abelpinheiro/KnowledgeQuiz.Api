using System.Diagnostics.Metrics;
using KnowledgeQuiz.Api.Application.Contracts;
using KnowledgeQuiz.Api.Application.DTOs;
using KnowledgeQuiz.Api.Domain.Enums;
using KnowledgeQuiz.Api.Infrastructure.Observability.Telemetry;
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
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserRepository repository, ILogger<AuthController> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    /// <summary>
    /// Login in the system
    /// </summary>
    /// <param name="loginRequest"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<string>>> LoginAsync(LoginRequest loginRequest)
    {
        _logger.LogInformation("Login attempt for email: {Email}", loginRequest.Email);
        AppMetrics.LoginAttempts.Add(1);
        
        var result = await _repository.LoginUserAsync(loginRequest);
        if (!result.Success)
        {
            _logger.LogWarning("Login failed for email: {Email}. Reason: {Reason}", loginRequest.Email, result.Reason);
            AppMetrics.LoginFailures.Add(1);
            
            var message = result.Reason switch
            {
                LoginFailureReason.InvalidCredentials => "Email or password is incorrect",
                _=> "Login failed"
            };
            
            return Unauthorized(ApiResponse<string>.Fail(message));
        }
        
        AppMetrics.LoginSuccesses.Add(1);
        _logger.LogInformation("Login successful for email: {Email}", loginRequest.Email);
        
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
        _logger.LogInformation("Registration attempt for email: {Email}", registerUserRequest.Email);
        AppMetrics.RegistrationAttempts.Add(1);
        
        var result = await _repository.RegisterUserAsync(registerUserRequest, "player");
      
        if (!result.Success)
        {
            _logger.LogWarning("Registration failed for email: {Email}. Reason: {Reason}", registerUserRequest.Email, result.FailureReason);
            AppMetrics.RegistrationFailures.Add(1);

            var message = result.FailureReason switch
            {
                RegisterFailureReason.UserAlreadyExists => "An account with this email already exists.",
                _ => "Registration failed."
            };
            
            return BadRequest(ApiResponse<string>.Fail(message));
        }
        
        _logger.LogInformation("Registration successful for email: {Email}", registerUserRequest.Email);
        AppMetrics.RegistrationSuccesses.Add(1);
        
        return Ok(ApiResponse<string>.SuccessResponse(null!, "Registration successful."));
    }
}