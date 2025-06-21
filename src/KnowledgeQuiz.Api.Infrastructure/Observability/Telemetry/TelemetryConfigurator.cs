using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace KnowledgeQuiz.Api.Infrastructure.Observability.Telemetry;

public static class TelemetryConfigurator
{
    public static IServiceCollection ConfigureTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["ServiceName"];
        var serviceVersion = configuration["ServiceVersion"];
        
        Log.Information("Configuring OpenTelemetry for service: {ServiceName}, version: {ServiceVersion}", serviceName, serviceVersion);

        services.AddOpenTelemetry()
            .WithMetrics(opt =>
            {
                Log.Information("Configuring OpenTelemetry Metrics");
                opt
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion))
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();
                Log.Information("OpenTelemetry Metrics configured");
            })
            .WithTracing(tracer =>
            {
                Log.Information("Configuring OpenTelemetry Tracing");
                tracer.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation();
                Log.Information("OpenTelemetry configuration completed");
            });
        
        return services;
    }
}