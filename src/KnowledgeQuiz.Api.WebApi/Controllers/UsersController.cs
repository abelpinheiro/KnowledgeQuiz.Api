using System.Diagnostics;
using System.Diagnostics.Metrics;
using KnowledgeQuiz.Api.Application.Contracts;
using KnowledgeQuiz.Api.Application.DTOs;
using KnowledgeQuiz.Api.Domain.Enums;
using KnowledgeQuiz.Api.Infrastructure.Observability.Telemetry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeQuiz.Api.WebApi.Controllers;

/// <summary>
/// Controller for handling all of user related logic
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository repository, ILogger<UsersController> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    //TODO endpoints for changing the password, changing the email, integration with email, delete user, get endpoint with filters and pagination
    
    /// <summary>
    /// Retrieve all users
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<string>>> GetUsersAsync()
    {
        _logger.LogInformation("GET /api/users - Retrieving all users");
        AppMetrics.UserFetches.Add(1);
        
        var result = await _repository.GetUsersAsync();

        _logger.LogInformation("GET /api/users - Retrieved {Count} users", result.Count);
        return Ok(ApiResponse<List<UserResponse>>.SuccessResponse(result, "Users successfully retrieved."));
    }

    /// <summary>
    /// Creates a new user with a role
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<string>>> CreateUserWithRoleAsync(RegisterUserWithRoleRequest request)
    {        
        _logger.LogInformation("POST /api/users - Attempting to create user with email: {Email} and role: {Role}", request.Email, request.Role);
        AppMetrics.RegistrationAttempts.Add(1);
        
        var result = await _repository.RegisterUserAsync(request, request.Role);

        if (!result.Success)
        {
            _logger.LogWarning("POST /api/users - Failed to create user with email: {Email}. Reason: {Reason}", request.Email, result.FailureReason);
            AppMetrics.RegistrationFailures.Add(1);
            
            var message = result.FailureReason switch
            {
                RegisterFailureReason.UserAlreadyExists => "An account with this email already exists.",
                RegisterFailureReason.InvalidRole => "The role provided is invalid.",
                _ => "Registration failed."
            };

            return BadRequest(ApiResponse<string>.Fail(message));
        }
        
        _logger.LogInformation("POST /api/users - Successfully created user with email: {Email}", request.Email);
        AppMetrics.RegistrationSuccesses.Add(1);
        
        return Ok(ApiResponse<string>.SuccessResponse(null!, "Registration successful."));
    }
    
    /// <summary>
    /// Assign a role to an existing user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="assignRoleRequest"></param>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpPut("{userId}/role")]
    public async Task<ActionResult<ApiResponse<string>>> AssignRoleToUser(int userId, AssignRoleRequest assignRoleRequest)
    {
        _logger.LogInformation("PUT /api/users/{UserId}/role - Assigning role: {Role} to userId: {UserId}", userId, assignRoleRequest.Role);
        AppMetrics.RoleAssignments.Add(1);
        
        var result = await _repository.AssignRoleToUserAsync(userId, assignRoleRequest);

        if (!result.Success)
        {
            _logger.LogWarning("PUT /api/users/{UserId}/role - Failed to assign role: {Role} to userId: {UserId}. Reason: {Reason}", userId, assignRoleRequest.Role, result.FailureReason);
            var message = result.FailureReason switch
            {
                RegisterFailureReason.InvalidRole => "The role provided is invalid.",
                _ => "Registration failed."
            };
            
            return BadRequest(ApiResponse<string>.Fail(message));
        }
        
        _logger.LogInformation("PUT /api/users/{UserId}/role - Successfully assigned role: {Role} to userId: {UserId}", userId, assignRoleRequest.Role);

        return Ok(ApiResponse<string>.SuccessResponse(null!, "Role update successful    ."));
    }
}