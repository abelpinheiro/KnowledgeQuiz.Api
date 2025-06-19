using HealthChecks.UI.Configuration;
using KnowledgeQuiz.Api.Infrastructure.DependencyInjection;
using KnowledgeQuiz.Api.Infrastructure.Observability.Logging;
using KnowledgeQuiz.Api.Infrastructure.Seeding;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.AddLogging(builder.Configuration);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.InfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Running in Development Environment");
    await app.SeedAsync();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Json Health endpoint
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
//app.MapPrometheusScrapingEndpoint();

app.Run();