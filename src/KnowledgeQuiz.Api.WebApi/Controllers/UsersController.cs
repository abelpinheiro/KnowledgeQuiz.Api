using KnowledgeQuiz.Api.Application.Contracts;
using KnowledgeQuiz.Api.Application.DTOs;
using KnowledgeQuiz.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = KnowledgeQuiz.Api.Application.DTOs.LoginRequest;

namespace KnowledgeQuiz.Api.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    //TODO endpoints for changing the password, changing the email, integration with email, delete user, get endpoint with filters and pagination

    #region General user endpoints
    /// <summary>
    /// Login in the system
    /// </summary>
    /// <param name="loginRequest"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<string>>> Login(LoginRequest loginRequest)
    {
        var result = await _userRepository.LoginUserAsync(loginRequest);
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
    public async Task<ActionResult<LoginResponse>> Register(RegisterUserRequest registerUserRequest)
    {
        var result = await _userRepository.RegisterUserAsync(registerUserRequest, "player");

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
    #endregion

    #region Admin level endpoints
    
    /// <summary>
    /// Retrieve all users
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<string>>> GetUsers()
    {
        var result = await _userRepository.GetUsersAsync();
        
        return Ok(ApiResponse<List<UserResponse>>.SuccessResponse(result, "Users successfully retrieved."));
    }

    /// <summary>
    /// Creates a new user with a role
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpPost("with-role")]
    public async Task<ActionResult<ApiResponse<string>>> RegisterUserWithRole(RegisterUserWithRoleRequest request)
    {
        var result = await _userRepository.RegisterUserAsync(request, request.Role);

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
        var result = await _userRepository.AssignRoleToUserAsync(userId, assignRoleRequest);

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
    #endregion
}