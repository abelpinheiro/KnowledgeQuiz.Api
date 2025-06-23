using System.Text;
using KnowledgeQuiz.Api.Application.Contracts;
using KnowledgeQuiz.Api.Infrastructure.Data;
using KnowledgeQuiz.Api.Infrastructure.ExceptionHandlers;
using KnowledgeQuiz.Api.Infrastructure.Observability;
using KnowledgeQuiz.Api.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace KnowledgeQuiz.Api.Infrastructure.DependencyInjection;

public static class ServiceContainer
{
    public static IServiceCollection InfrastructureServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        Log.Information("Starting InfrastructureServices configuration");
        services
            .AddJwtAuthentication(configuration)
            .AddDatabase(configuration)
            .AddObservability(configuration, environment)
            .AddRepositories()
            .AddExceptionHandlers();

        return services;
    }

    private static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {
        Log.Information("Configuring exception handlers");
        services.AddExceptionHandler<UnauthorizedExceptionHandler>();
        services.AddExceptionHandler<BadRequestExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddProblemDetails();
        Log.Information("Exception handlers configured");
        return services;
    }
    
    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Information("Configuring database context with PostgreSQL provider");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default"), 
                b => b.MigrationsAssembly(typeof(ServiceContainer).Assembly.FullName)));
        
        Log.Information("Database configured");
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        Log.Information("Configuring DI repositories");
        services.AddScoped<IUserRepository, UserRepository>();
        Log.Information("DI repositories configured");
        return services;
    }


    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Information("Configuring JWT Authentication");
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
        ).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
            };
        });
        
        services.AddAuthorization();
        Log.Information("JWT Authentication configured");
        return services;
    }
}