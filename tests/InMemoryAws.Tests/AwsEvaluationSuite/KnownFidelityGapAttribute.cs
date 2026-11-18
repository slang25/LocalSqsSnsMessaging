namespace InMemoryAws.Tests.AwsEvaluationSuite;

/// <summary>
/// Root causes of the evaluation-suite checks InMemoryAws does not pass yet, as triaged in
/// the fidelity report. Each <see cref="KnownFidelityGapAttribute"/> names one.
/// </summary>
public enum FidelityGap
{
    /// <summary>Validation AWS performs that is deliberately not enforced (the purge and re-create cooldowns).</summary>
    Validation,

    /// <summary>A one-off behavioral difference, kept for compatibility.</summary>
    Other,

    /// <summary>
    /// A queue or topic whose policy grants EventBridge nothing still receives EventBridge's
    /// deliveries, kept deliberately as for SNS: most test setups add no policy. A policy that
    /// denies EventBridge, or admits it only for other rules, is enforced.
    /// </summary>
    EventBridgeUngrantedTargets,

    /// <summary>
    /// The check's own waits outlast the runner's 60-second budget for an emulator, so it fails
    /// against any emulator, however fast; against AWS it runs with a longer budget.
    /// </summary>
    EmulatorBudget,

    /// <summary>DynamoDB provisioned throughput is reported, never enforced: nothing is throttled.</summary>
    DynamoDbThroughput,

    /// <summary>DynamoDB requests are not authenticated: SigV4 signatures and missing credentials are not checked.</summary>
    DynamoDbAuthentication,

    /// <summary>A one-off DynamoDB difference: a message's wording, the order of checks, an edge case.</summary>
    DynamoDbDetail,

    /// <summary>S3 requests are not authenticated: SigV4 signatures, presigned URLs' signatures and clock skew are not checked.</summary>
    S3Authentication,

    /// <summary>
    /// S3 access control the library doesn't enforce: an ACL, a bucket policy statement or a Block
    /// Public Access setting that would refuse (or admit) a caller.
    /// </summary>
    S3AccessControl,

    /// <summary>
    /// Work S3 does in the background after a request returns (lifecycle transitions and expiry,
    /// replication, restores, inventory and log delivery), which the library doesn't run.
    /// </summary>
    S3BackgroundWork,

    /// <summary>A one-off S3 difference: a message's wording, the order of checks, an edge case.</summary>
    S3Detail,
}

/// <summary>
/// Marks an evaluation-suite check that InMemoryAws does not pass yet, so it is skipped and
/// the suite stays green. Set <c>INMEMORYAWS_RUN_KNOWN_GAPS=1</c> to run these tests anyway: a test
/// that then passes means its gap has been fixed and the attribute can be removed.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class KnownFidelityGapAttribute : SkipAttribute
{
    public const string RunKnownGapsVariable = "INMEMORYAWS_RUN_KNOWN_GAPS";

    public KnownFidelityGapAttribute(FidelityGap gap, string detail)
        : base($"Known fidelity gap ({gap}): {detail}")
    {
        Gap = gap;
        Detail = detail;
    }

    public FidelityGap Gap { get; }

    public string Detail { get; }

    public override Task<bool> ShouldSkip(TestRegisteredContext context)
        => Task.FromResult(Environment.GetEnvironmentVariable(RunKnownGapsVariable) is not ("1" or "true"));
}
