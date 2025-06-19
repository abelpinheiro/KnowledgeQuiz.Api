using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace KnowledgeQuiz.Api.Infrastructure.Observability.Logging;

public static class LoggingConfigurator
{
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/knowledgequiz-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }
}