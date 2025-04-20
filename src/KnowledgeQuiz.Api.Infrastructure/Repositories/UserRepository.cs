using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KnowledgeQuiz.Api.Application.Contracts;
using KnowledgeQuiz.Api.Application.DTOs;
using KnowledgeQuiz.Api.Domain.Entities;
using KnowledgeQuiz.Api.Domain.Enums;
using KnowledgeQuiz.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KnowledgeQuiz.Api.Infrastructure.Repositories;

/// <summary>
/// Repository for Users
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public UserRepository(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Register a user on the database.
    /// </summary>
    /// <param name="registerUserRequest">Register user DTO</param>
    /// <param name="userRole">Role of new user</param>
    /// <returns></returns>
    public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest registerUserRequest, string userRole)
    {
        var user = await FindUserByEmail(registerUserRequest.Email);
        if (user != null)
            return new RegisterUserResponse(false, RegisterFailureReason.UserAlreadyExists);

        var role = await _context.Roles.FirstOrDefaultAsync(x => x.Name.ToLower() == userRole.ToLower());

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
        return new RegisterUserResponse(true);
    }

    /// <summary>
    /// Login to the system. Return JWT token if successful
    /// </summary>
    /// <param name="loginRequest">Login DTO</param>
    /// <returns></returns>
    public async Task<LoginResponse> LoginUserAsync(LoginRequest loginRequest)
    {
        var user = await FindUserByEmail(loginRequest.Email);
        if (user == null)
            return new LoginResponse(false, null, LoginFailureReason.InvalidCredentials);
        
        bool checkPassword = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password);
        if (!checkPassword)
            return new LoginResponse(false, null, LoginFailureReason.InvalidCredentials);
        
        return new LoginResponse(true, GenerateJwtToken(user));
    }

    /// <summary>
    /// Helper to find an email on the users table
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns></returns>
    private async Task<User?> FindUserByEmail(string email)
    {
        return await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Method to generate a JWT Token
    /// </summary>
    /// <param name="user">User data</param>
    /// <returns></returns>
    private string GenerateJwtToken(User user)
    {
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

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public async Task<List<UserResponse>> GetUsersAsync()
    {
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

        return result;
    }

    public async Task<RegisterUserResponse> AssignRoleToUserAsync(int userId, AssignRoleRequest assignRoleRequest)
    {
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return new RegisterUserResponse(false, RegisterFailureReason.UserNotFound);
        
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == assignRoleRequest.Role.ToLower());
        if (role == null) return new RegisterUserResponse(false, RegisterFailureReason.InvalidRole);
        
        user.RoleId = role.Id;
        user.Role = role;
        
        await _context.SaveChangesAsync();
        return new RegisterUserResponse(true);
    }
}