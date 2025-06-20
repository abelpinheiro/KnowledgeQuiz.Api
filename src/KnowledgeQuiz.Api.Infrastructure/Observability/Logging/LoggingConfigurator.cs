using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace KnowledgeQuiz.Api.Infrastructure.Observability.Logging;

public static class LoggingConfigurator
{
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        var environment = builder.Environment.EnvironmentName;
        var elasticUri = configuration["ElasticConfiguration:Uri"];
        var username = configuration["ElasticConfiguration:Username"];
        var password = configuration["ElasticConfiguration:Password"];


        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext();

        if (environment == "Development")
        {
            loggerConfig.WriteTo.Console();
        }
        else
        {
            if (!string.IsNullOrEmpty(elasticUri) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                loggerConfig
                    .WriteTo.File("logs/knowledgequiz-.log", rollingInterval: RollingInterval.Day)
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                    {
                        AutoRegisterTemplate = true,
                        IndexFormat = "random-{0:yyyy.MM.dd}",
                        InlineFields = true,
                        ModifyConnectionSettings = conn => conn.BasicAuthentication(username, password),
                        MinimumLogEventLevel = LogEventLevel.Warning
                    });
            }
        }
        
        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();
        return builder;
    }
}