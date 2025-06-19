using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace KnowledgeQuiz.Api.Infrastructure.Observability.Telemetry;

public static class TelemetryConfigurator
{
    public static IServiceCollection ConfigureTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["ServiceName"];
        var serviceVersion = configuration["ServiceVersion"];

        services.AddOpenTelemetry()
            .WithMetrics(opt =>
            {
                opt
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion))
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();
            })
            .WithTracing(tracer =>
            {
                tracer.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation();
            });;
        
        return services;
    }
}