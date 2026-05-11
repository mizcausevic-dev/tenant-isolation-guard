using TenantIsolationGuard.Api.Data;
using TenantIsolationGuard.Api.Models;

namespace TenantIsolationGuard.Api.Services;

public sealed class IsolationAnalysisService
{
    private readonly IReadOnlyList<TenantProfile> _tenants = SampleIsolationData.Tenants;
    private readonly IReadOnlyList<RequestEvaluation> _requests = SampleIsolationData.Requests;

    public IReadOnlyList<TenantProfile> GetTenants() => _tenants;

    public IReadOnlyList<RequestEvaluation> GetRequests() =>
        _requests.OrderByDescending(item => item.EvaluatedAt).ToArray();

    public RequestEvaluation GetRequest(string requestId) =>
        _requests.FirstOrDefault(item => item.RequestId.Equals(requestId, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException($"Request not found: {requestId}");

    public DashboardSummary GetSummary()
    {
        var criticalRequests = _requests.Count(item => item.Severity == IsolationSeverity.Critical);
        var shadowAdminPaths = _tenants.Count(item => item.HasShadowAdminPath);
        var crossRegionBridges = _requests.Count(item => item.Flags.Contains("cross-region"));
        var tiers = _tenants.Select(item => item.IsolationTier).Distinct().Order().ToArray();

        return new DashboardSummary(
            _tenants.Count,
            criticalRequests,
            shadowAdminPaths,
            crossRegionBridges,
            tiers);
    }

    public IsolationAnalysisResponse Analyze(IsolationAnalysisInput input)
    {
        var score = 16;
        var issues = new List<string>();
        var passedChecks = new List<string>();

        if (!input.SourceTenantId.Equals(input.TargetTenantId, StringComparison.OrdinalIgnoreCase))
        {
            score += 20;
            issues.Add("The request crosses a tenant boundary.");
        }
        else
        {
            passedChecks.Add("The request stays within one tenant boundary.");
        }

        if (input.CrossesRegionBoundary)
        {
            score += 18;
            issues.Add("The request crosses a regional data boundary.");
        }
        else
        {
            passedChecks.Add("No regional data boundary is crossed.");
        }

        if (input.TouchesRestrictedDataset)
        {
            score += 20;
            issues.Add("The path touches restricted data that should not move laterally.");
        }
        else
        {
            passedChecks.Add("The path does not touch restricted data.");
        }

        if (input.HasSharedCredential)
        {
            score += 12;
            issues.Add("A shared credential still exists in the request path.");
        }
        else
        {
            passedChecks.Add("No shared credential is attached to the request path.");
        }

        if (input.HasPrivilegedBridge)
        {
            score += 16;
            issues.Add("A privileged bridge can widen the blast radius if the path is abused.");
        }
        else
        {
            passedChecks.Add("No privileged bridge is attached to this request path.");
        }

        if (input.CallerRole.Contains("admin", StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
            issues.Add("The caller role carries elevated administrative capability.");
        }

        if (input.ExistingControls.Intersect(new[] { "tenant-scope-claims", "workload-identity", "breakglass-approval" }).Count() >= 2)
        {
            score -= 8;
            passedChecks.Add("Multiple meaningful controls already reduce path expansion risk.");
        }
        else
        {
            issues.Add("Control coverage is too thin for the current request shape.");
        }

        score = Math.Clamp(score, 5, 98);
        var status = score >= 72 ? "escalate" : score >= 46 ? "watch" : "stable";
        var nextAction = status switch
        {
            "escalate" => "Route the request path to platform security and revoke or re-scope the bridge before further access is allowed.",
            "watch" => "Keep the request in the isolation review queue and validate claims, identities, and boundary intent before renewal.",
            _ => "Maintain normal tenant-boundary monitoring and record the current control posture."
        };

        return new IsolationAnalysisResponse(status, score, issues, passedChecks, nextAction);
    }
}
