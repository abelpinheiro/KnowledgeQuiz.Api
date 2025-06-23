using HealthChecks.UI.Configuration;
using KnowledgeQuiz.Api.Infrastructure.Observability.HealthChecks;
using KnowledgeQuiz.Api.Infrastructure.Observability.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KnowledgeQuiz.Api.Infrastructure.Observability;

public static class ObservabilityConfigurator
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {        
        Log.Information("Starting observability configuration");
        services
            .ConfigureHealthChecks(configuration, environment)
            .ConfigureTelemetry(configuration);
        
        return services;
    }

    public static void UseObservabilityEndpoints(this WebApplication app)
    {
        Log.Information("Mapping observability endpoints");
        app.MapHealthChecks("/api/health", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description?.ToString(),
                        duration = e.Value.Duration.ToString(),
                        exception = e.Value.Exception?.Message
                    })
                });
        
                await context.Response.WriteAsync(result);
            }
        });

        app.MapHealthChecksUI(delegate(Options options)
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-ui-api";
            //options.AddCustomStylesheet("./HealthCheck/Custom.css");
        });

        // Map metrics endpoint
        app.UseOpenTelemetryPrometheusScrapingEndpoint();
        Log.Information("Observability endpoints mapped");
    }
}