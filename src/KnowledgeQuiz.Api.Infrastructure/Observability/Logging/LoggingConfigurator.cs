using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
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

        loggerConfig
            .WriteTo.File("logs/knowledgequiz-.log", rollingInterval: RollingInterval.Day);
        
        if (!string.IsNullOrEmpty(elasticUri) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            loggerConfig
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "knowledgequiz-api-logs-{0:yyyy.MM.dd}",
                    InlineFields = true,
                    ModifyConnectionSettings = conn => conn.BasicAuthentication(username, password),
                    MinimumLogEventLevel = LogEventLevel.Warning
                });
        }
        else
        {
            Console.WriteLine("Elasticsearch not configured. Skipping Elastic logging...");
        }


        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();
        return builder;
    }
}