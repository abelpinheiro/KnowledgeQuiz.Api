using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace KnowledgeQuiz.Api.Infrastructure.Observability.HealthChecks;

public static class HealthChecksConfigurator
{
    public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Information("Configuring health checks");
        
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrEmpty(connectionString))
        {
            Log.Warning("Database connection string for Health Checks is not set");
        }
        else
        {
            Log.Information("Adding database Health Check for connection string: {ConnectionString}", connectionString);
            services.AddHealthChecks()
                .AddNpgSql(connectionString, name: "PostgreSQL");
        }

        Log.Information("Configuring Health Checks UI");
        services.AddHealthChecksUI(opt =>
        {
            opt.SetEvaluationTimeInSeconds(10); // Time in seconds between check
            opt.MaximumHistoryEntriesPerEndpoint(60); // Maximum history checks
            opt.SetApiMaxActiveRequests(1); // Api requests concurrency
            opt.AddHealthCheckEndpoint("KnowledgeQuiz API Health", "/api/health");
        })
        .AddInMemoryStorage();
        
        Log.Information("Health Checks configuration completed");

        return services;
    }
}