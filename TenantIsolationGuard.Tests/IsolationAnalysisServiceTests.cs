using TenantIsolationGuard.Api.Models;
using TenantIsolationGuard.Api.Services;

namespace TenantIsolationGuard.Tests;

public sealed class IsolationAnalysisServiceTests
{
    private readonly IsolationAnalysisService _service = new();

    [Fact]
    public void Summary_ReturnsTrackedTenantCounts()
    {
        var summary = _service.GetSummary();

        Assert.Equal(4, summary.TenantsTracked);
        Assert.True(summary.CriticalRequests >= 1);
    }

    [Fact]
    public void Analyze_EscalatesCrossRegionRestrictedBridge()
    {
        var response = _service.Analyze(new IsolationAnalysisInput(
            "tn-core-us",
            "tn-health-apac",
            "platform-admin",
            "ops-breakglass",
            true,
            true,
            true,
            true,
            new[] { "tenant-scope-claims", "breakglass-approval" }));

        Assert.Equal("escalate", response.Status);
        Assert.True(response.Score >= 72);
    }
}
