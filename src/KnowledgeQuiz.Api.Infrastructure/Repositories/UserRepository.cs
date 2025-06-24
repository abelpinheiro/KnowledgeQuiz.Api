using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KnowledgeQuiz.Api.Application.Contracts;
using KnowledgeQuiz.Api.Application.DTOs;
using KnowledgeQuiz.Api.Domain.Entities;
using KnowledgeQuiz.Api.Domain.Enums;
using KnowledgeQuiz.Api.Infrastructure.Data;
using KnowledgeQuiz.Api.Infrastructure.Observability.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace KnowledgeQuiz.Api.Infrastructure.Repositories;

/// <summary>
/// Repository for Users
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserRepository> _logger;
    private static readonly ActivitySource ActivitySource = new("KnowledgeQuiz.Api.UserRepository");

    public UserRepository(AppDbContext context, IConfiguration configuration, ILogger<UserRepository> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Register a user on the database.
    /// </summary>
    /// <param name="registerUserRequest">Register user DTO</param>
    /// <param name="userRole">Role of new user</param>
    /// <returns></returns>
    public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest registerUserRequest, string userRole)
    {
        using var activity = ActivitySource.StartActivity("RegisterUserAsync");

        _logger.LogInformation("Attempting to register user with email: {Email} and role: {Role}", registerUserRequest.Email, userRole);
        
        var user = await FindUserByEmail(registerUserRequest.Email);
        if (user != null)
        {
            _logger.LogWarning("Registration failed. User with email {Email} already exists",
                registerUserRequest.Email);
            return new RegisterUserResponse(false, RegisterFailureReason.UserAlreadyExists);
        }

        var role = await _context.Roles.FirstOrDefaultAsync(x => x.Name.ToLower() == userRole.ToLower());
        
        if (role == null)
        {
            _logger.LogWarning("Registration failed. Role '{Role}' does not exist", userRole);
            return new RegisterUserResponse(false, RegisterFailureReason.InvalidRole);
        }
        
        await _context.Users.AddAsync(new User()
        {
            Name = registerUserRequest.Name,
            Email = registerUserRequest.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(registerUserRequest.Password),
            DateOfBirth = registerUserRequest.DateOfBirth,
            RoleId = role.Id,
            Role = role
        });
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("User with email {Email} successfully registered.", registerUserRequest.Email);
        return new RegisterUserResponse(true);
    }

    /// <summary>
    /// Login to the system. Return JWT token if successful
    /// </summary>
    /// <param name="loginRequest">Login DTO</param>
    /// <returns></returns>
    public async Task<LoginResponse> LoginUserAsync(LoginRequest loginRequest)
    {
        _logger.LogInformation("Payload recebido: Email={Email}, Password={Password}", loginRequest.Email, loginRequest.Password);

        using var activity = ActivitySource.StartActivity("LoginUserAsync");
        activity?.SetTag("user.email", loginRequest.Email);
        
        _logger.LogInformation("Login attempt for email: {Email}", loginRequest.Email);
        var user = await FindUserByEmail(loginRequest.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed. No user found with email: {Email}", loginRequest.Email);
            return new LoginResponse(false, null, LoginFailureReason.InvalidCredentials);
        }

        bool passwordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password);
        if (!passwordValid)
        {            
            _logger.LogWarning("Login failed for email: {Email}. Invalid password.", loginRequest.Email);
            return new LoginResponse(false, null, LoginFailureReason.InvalidCredentials);
        }

        var token = GenerateJwtToken(user);
        _logger.LogInformation("Login successful for email: {Email}", loginRequest.Email);
        return new LoginResponse(true, token);
    }

    /// <summary>
    /// Helper to find an email on the users table
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns></returns>
    private async Task<User?> FindUserByEmail(string email)
    {
        AppMetrics.DbUserQueries.Add(1, new KeyValuePair<string, object>("operation", "FindUserByEmail"));
        using var activity = ActivitySource.StartActivity("FindUserByEmail");
        activity?.SetTag("user.email", email);
        
        return await MetricsHelper.TrackDurationAsync(
            AppMetrics.OperationDurationHistogram,
            async () => await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(u => u.Email == email),
            new KeyValuePair<string, object>("operation", "FindUserByEmail")
        );
    }

    /// <summary>
    /// Method to generate a JWT Token
    /// </summary>
    /// <param name="user">User data</param>
    /// <returns></returns>
    private string GenerateJwtToken(User user)
    {
        using var activity = ActivitySource.StartActivity("GenerateJwtToken");
        activity?.SetTag("user.id", user.Id);
        activity?.SetTag("user.email", user.Email);
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Name!),
            new Claim(ClaimTypes.NameIdentifier, user.Email!),
            new Claim(ClaimTypes.Role, user.Role!.Name)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: userClaims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
            );
        
        _logger.LogInformation("JWT token generated for userId: {UserId}, email: {Email}", user.Id, user.Email);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public async Task<List<UserResponse>> GetUsersAsync()
    {
        using var activity = ActivitySource.StartActivity("GetUsersAsync");
        _logger.LogInformation("Fetching all users from database");
        var users = await _context.Users.Include(user => user.Role).ToListAsync();
        var result = new List<UserResponse>();
        foreach (var user in users)
        {
            result.Add(new UserResponse()
            {
                Id = user.Id,
                Name = user.Name,
                CreatedAt = user.CreatedAt,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                IsAnonymous = user.IsAnonymous,
                Role = user.Role!.Name
            });
        }
        
        _logger.LogInformation("Retrieved {Count} users from database", result.Count);
        return result;
    }

    public async Task<RegisterUserResponse> AssignRoleToUserAsync(int userId, AssignRoleRequest assignRoleRequest)
    {
        using var activity = ActivitySource.StartActivity("AssignRoleToUserAsync");
        activity?.SetTag("user.id", userId);
        activity?.SetTag("new.role", assignRoleRequest.Role);
            
        _logger.LogInformation("Assigning role '{Role}' to userId: {UserId}", assignRoleRequest.Role, userId);
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            _logger.LogWarning("User with id {UserId} not found", userId);
            return new RegisterUserResponse(false, RegisterFailureReason.UserNotFound);
        }
        
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == assignRoleRequest.Role.ToLower());
        if (role == null)
        {
            _logger.LogWarning("Role '{Role}' not found in database", assignRoleRequest.Role);
            return new RegisterUserResponse(false, RegisterFailureReason.InvalidRole);
        }
        
        user.RoleId = role.Id;
        user.Role = role;
        
        await MetricsHelper.TrackDurationAsync(
            AppMetrics.OperationDurationHistogram,
            async () => await _context.SaveChangesAsync(),
            new KeyValuePair<string, object>("operation", "SaveChangesAsync"),
            new KeyValuePair<string, object>("action", "AssignRole")
        );
        
        _logger.LogInformation("Role '{Role}' successfully assigned to userId: {UserId}", assignRoleRequest.Role, userId);
        return new RegisterUserResponse(true);
    }
}