using KnowledgeQuiz.Api.Infrastructure.Observability.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeQuiz.Api.Infrastructure.Observability;

public static class ObservabilityConfigurator
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAppHealthChecks(configuration);
        
        return services;
    }
}