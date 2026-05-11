using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TenantIsolationGuard.Tests;

public sealed class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Root_ReturnsServiceInfo()
    {
        var payload = await _client.GetFromJsonAsync<Dictionary<string, string>>("/");

        Assert.NotNull(payload);
        Assert.Equal("tenant-isolation-guard", payload["service"]);
    }

    [Fact]
    public async Task Summary_ReturnsDashboardShape()
    {
        var payload = await _client.GetFromJsonAsync<Dictionary<string, object>>("/api/dashboard/summary");

        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("tenantsTracked"));
    }
}
