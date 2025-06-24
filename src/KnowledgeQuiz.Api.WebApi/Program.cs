using System.Text.Json;
using KnowledgeQuiz.Api.Infrastructure.DependencyInjection;
using KnowledgeQuiz.Api.Infrastructure.Observability;
using KnowledgeQuiz.Api.Infrastructure.Observability.Logging;
using KnowledgeQuiz.Api.Infrastructure.Seeding;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.AddLogging(builder.Configuration);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });;

var allowedOriginsString = builder.Configuration["AllowedOrigins"];
var allowedOrigins = allowedOriginsString?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins!) // ou a URL do seu frontend
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();
builder.Services.InfrastructureServices(builder.Configuration, builder.Environment);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5203";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Logger instance
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Application is starting...");

app.UseExceptionHandler();

await app.SeedAsync(logger);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    logger.LogInformation("Running in Development Environment");
    app.MapOpenApi();
    app.MapScalarApiReference();
    
    logger.LogInformation("HTTPS Redirection ENABLED (Development only)");
    //app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapPrometheusScrapingEndpoint();

app.UseObservabilityEndpoints();

app.MapFallback(() => Results.NotFound("Endpoint not found."));
app.MapGet("/", () => "API Online");

await app.RunAsync();