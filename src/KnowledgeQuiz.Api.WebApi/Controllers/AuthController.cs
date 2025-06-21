using System.Diagnostics.Metrics;
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
    private readonly ILogger<AuthController> _logger;
    private static readonly Meter Meter = new("KnowledgeQuiz.Api", "1.0.0");
    private static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>("custom_requests_total");


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
        RequestCounter.Add(1); // JUST FOR TESTING
        
        var result = await _repository.LoginUserAsync(loginRequest);
        if (!result.Success)
        {
            _logger.LogWarning("Login failed for email: {Email}. Reason: {Reason}", loginRequest.Email, result.Reason);
            var message = result.Reason switch
            {
                LoginFailureReason.InvalidCredentials => "Email or password is incorrect",
                _=> "Login failed"
            };
            
            return Unauthorized(ApiResponse<string>.Fail(message));
        }
        
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
        var result = await _repository.RegisterUserAsync(registerUserRequest, "player");

        if (!result.Success)
        {
            _logger.LogWarning("Registration failed for email: {Email}. Reason: {Reason}", registerUserRequest.Email, result.FailureReason);
            var message = result.FailureReason switch
            {
                RegisterFailureReason.UserAlreadyExists => "An account with this email already exists.",
                _ => "Registration failed."
            };
            
            _logger.LogInformation("Registration successful for email: {Email}", registerUserRequest.Email);
            return BadRequest(ApiResponse<string>.Fail(message));
        }
        
        return Ok(ApiResponse<string>.SuccessResponse(null!, "Registration successful."));
    }
}