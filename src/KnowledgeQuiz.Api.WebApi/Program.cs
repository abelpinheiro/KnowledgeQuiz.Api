using KnowledgeQuiz.Api.Infrastructure.DependencyInjection;
using KnowledgeQuiz.Api.Infrastructure.Observability;
using KnowledgeQuiz.Api.Infrastructure.Observability.Logging;
using KnowledgeQuiz.Api.Infrastructure.Seeding;
using Scalar.AspNetCore;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.AddLogging(builder.Configuration);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.InfrastructureServices(builder.Configuration);

var app = builder.Build();

// Logger instance
var logger = app.Services.GetRequiredService<ILogger>();

logger.Information("Application is starting...");

app.UseExceptionHandler();
await app.SeedAsync(logger);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    logger.Information("Running in Development Environment");
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//app.MapPrometheusScrapingEndpoint();

app.UseObservabilityEndpoints();
app.Run();