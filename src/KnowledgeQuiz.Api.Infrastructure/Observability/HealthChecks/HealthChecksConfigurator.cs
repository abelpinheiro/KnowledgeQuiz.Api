using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeQuiz.Api.Infrastructure.Observability.HealthChecks;

public static class HealthChecksConfigurator
{
    public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Default"), name: "SQL Server");

        services.AddHealthChecksUI(opt =>
        {
            opt.SetEvaluationTimeInSeconds(10); // Time in seconds between check
            opt.MaximumHistoryEntriesPerEndpoint(60); // Maximum history checks
            opt.SetApiMaxActiveRequests(1); // Api requests concurrency
            opt.AddHealthCheckEndpoint("KnowledgeQuiz API Health", "/api/health");
        })
        .AddSqlServerStorage(configuration.GetConnectionString("Default"));
        
        return services;
    }
}