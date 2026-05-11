using TenantIsolationGuard.Api.Models;

namespace TenantIsolationGuard.Api.Data;

public static class SampleIsolationData
{
    public static IReadOnlyList<TenantProfile> Tenants => new[]
    {
        new TenantProfile("tn-core-us", "Core United States", "us-east", "strict", "Platform Security", false, false),
        new TenantProfile("tn-finance-eu", "Finance Europe", "eu-west", "regulated", "Finance Controls", true, false),
        new TenantProfile("tn-health-apac", "Health APAC", "ap-southeast", "restricted", "Trust + Compliance", true, true),
        new TenantProfile("tn-sandbox-us", "Sandbox United States", "us-west", "standard", "Developer Experience", false, true)
    };

    public static IReadOnlyList<RequestEvaluation> Requests => new[]
    {
        new RequestEvaluation(
            "req-9001",
            "tn-sandbox-us",
            "tn-core-us",
            "support-admin",
            "shared-inspector",
            "escalate",
            IsolationSeverity.High,
            "Support tooling attempted to reach a production tenant through a shared inspection path.",
            new[] { "cross-tenant", "shared-path", "support-admin" },
            new DateTimeOffset(2026, 5, 11, 10, 15, 0, TimeSpan.Zero)),
        new RequestEvaluation(
            "req-9002",
            "tn-finance-eu",
            "tn-finance-eu",
            "finance-reviewer",
            "tenant-native-api",
            "allow",
            IsolationSeverity.Low,
            "Finance tenant access stayed inside the same regulated boundary with no bridge expansion.",
            new[] { "same-tenant", "regulated-lane" },
            new DateTimeOffset(2026, 5, 11, 11, 05, 0, TimeSpan.Zero)),
        new RequestEvaluation(
            "req-9003",
            "tn-core-us",
            "tn-health-apac",
            "platform-admin",
            "ops-breakglass",
            "deny",
            IsolationSeverity.Critical,
            "A privileged breakglass path tried to cross both region and restricted-data boundaries.",
            new[] { "cross-region", "restricted-dataset", "breakglass" },
            new DateTimeOffset(2026, 5, 11, 11, 42, 0, TimeSpan.Zero)),
        new RequestEvaluation(
            "req-9004",
            "tn-health-apac",
            "tn-health-apac",
            "tenant-operator",
            "analytics-proxy",
            "watch",
            IsolationSeverity.Medium,
            "Tenant-local traffic is stable, but the analytics proxy still shares an elevated service account.",
            new[] { "shared-credential", "analytics-proxy" },
            new DateTimeOffset(2026, 5, 11, 12, 12, 0, TimeSpan.Zero))
    };
}
