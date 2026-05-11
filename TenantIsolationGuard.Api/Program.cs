using Microsoft.OpenApi.Models;
using TenantIsolationGuard.Api.Models;
using TenantIsolationGuard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IsolationAnalysisService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tenant Isolation Guard",
        Version = "v1",
        Description = "Tenant-boundary policy evaluation and cross-tenant risk scoring service."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "docs";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tenant Isolation Guard v1");
});

app.MapGet("/", () => Results.Ok(new
{
    status = "ok",
    service = "tenant-isolation-guard",
    docs = "/docs"
})).WithOpenApi();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithOpenApi();

app.MapGet("/api/tenants", (IsolationAnalysisService service) => Results.Ok(service.GetTenants())).WithOpenApi();

app.MapGet("/api/requests", (IsolationAnalysisService service) => Results.Ok(service.GetRequests())).WithOpenApi();

app.MapGet("/api/requests/{requestId}", (string requestId, IsolationAnalysisService service) =>
{
    try
    {
        return Results.Ok(service.GetRequest(requestId));
    }
    catch (InvalidOperationException exception)
    {
        return Results.NotFound(new { error = exception.Message });
    }
}).WithOpenApi();

app.MapGet("/api/dashboard/summary", (IsolationAnalysisService service) => Results.Ok(service.GetSummary())).WithOpenApi();

app.MapPost("/api/analyze/isolation", (IsolationAnalysisInput input, IsolationAnalysisService service) =>
    Results.Ok(service.Analyze(input))).WithOpenApi();

app.Run();

public partial class Program;
