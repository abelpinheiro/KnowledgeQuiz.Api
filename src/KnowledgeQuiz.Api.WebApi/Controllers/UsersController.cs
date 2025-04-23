using KnowledgeQuiz.Api.Application.Contracts;
using KnowledgeQuiz.Api.Application.DTOs;
using KnowledgeQuiz.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = KnowledgeQuiz.Api.Application.DTOs.LoginRequest;

namespace KnowledgeQuiz.Api.WebApi.Controllers;

/// <summary>
/// Controller for handling all of user related logic
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;

    public UsersController(IUserRepository repository)
    {
        _repository = repository;
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
        var result = await _repository.GetUsersAsync();
        
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
        var result = await _repository.RegisterUserAsync(request, request.Role);

        if (!result.Success)
        {
            var message = result.FailureReason switch
            {
                RegisterFailureReason.UserAlreadyExists => "An account with this email already exists.",
                RegisterFailureReason.InvalidRole => "The role provided is invalid.",
                _ => "Registration failed."
            };

            return BadRequest(ApiResponse<string>.Fail(message));
        }
        
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
        var result = await _repository.AssignRoleToUserAsync(userId, assignRoleRequest);

        if (!result.Success)
        {
            var message = result.FailureReason switch
            {
                RegisterFailureReason.InvalidRole => "The role provided is invalid.",
                _ => "Registration failed."
            };
            
            return BadRequest(ApiResponse<string>.Fail(message));
        }
        
        return Ok(ApiResponse<string>.SuccessResponse(null!, "Role update successful    ."));
    }
}