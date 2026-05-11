namespace TenantIsolationGuard.Api.Models;

public enum IsolationSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public sealed record TenantProfile(
    string TenantId,
    string TenantName,
    string DataRegion,
    string IsolationTier,
    string OwnerLane,
    bool HasPrivilegedBridge,
    bool HasShadowAdminPath);

public sealed record RequestEvaluation(
    string RequestId,
    string SourceTenantId,
    string TargetTenantId,
    string CallerRole,
    string AccessPath,
    string Decision,
    IsolationSeverity Severity,
    string Summary,
    IReadOnlyList<string> Flags,
    DateTimeOffset EvaluatedAt);

public sealed record DashboardSummary(
    int TenantsTracked,
    int CriticalRequests,
    int ShadowAdminPaths,
    int CrossRegionBridges,
    IReadOnlyList<string> IsolationTiers);

public sealed record IsolationAnalysisInput(
    string SourceTenantId,
    string TargetTenantId,
    string CallerRole,
    string AccessPath,
    bool HasSharedCredential,
    bool HasPrivilegedBridge,
    bool CrossesRegionBoundary,
    bool TouchesRestrictedDataset,
    IReadOnlyList<string> ExistingControls);

public sealed record IsolationAnalysisResponse(
    string Status,
    int Score,
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> PassedChecks,
    string RecommendedNextAction);
