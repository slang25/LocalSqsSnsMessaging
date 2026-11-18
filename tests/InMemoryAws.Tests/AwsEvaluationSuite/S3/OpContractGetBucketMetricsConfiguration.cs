// Translated from behavioral/s3/check_op_contract_get_bucket_metrics_configuration.py
// (https://github.com/aws/aws-emulator-evaluation-suite, commit 4df2823).
using Amazon.S3;
using Amazon.S3.Model;
using static InMemoryAws.Tests.AwsEvaluationSuite.FidelityAssert;

namespace InMemoryAws.Tests.AwsEvaluationSuite.S3;

/// <summary>s3.op-contract-get-bucket-metrics-configuration</summary>
[Category("AwsEvaluationSuite")]
public sealed class OpContractGetBucketMetricsConfiguration
{
    /// <summary>
    /// GetBucketMetricsConfiguration returns the named filter config; an unknown Id raises NoSuchConfiguration.
    /// </summary>
    [Test]
    [DisplayName("s3.op-contract-get-bucket-metrics-configuration")]
    [Timeout(FidelityContext.HangTimeoutMilliseconds)]
    public async Task Run(CancellationToken cancellationToken)
    {
        await using var ctx = new FidelityContext(FidelityContext.BudgetMilliseconds, cancellationToken);
        var s3 = ctx.S3;
        var bucket = await S3Fidelity.CreateBucket(ctx, "sim2real-behav-");
        const string metricsId = "sim2real-metrics";

        // --- error boundary first: no configuration exists for this Id yet ---
        var (code, message) = await ClientError(() => s3.GetBucketMetricsConfigurationAsync(new GetBucketMetricsConfigurationRequest { BucketName = bucket, MetricsId = metricsId }));
        Ensure(code == "NoSuchConfiguration", $"expected NoSuchConfiguration, got {Py.Repr(code)}");
        Ensure(message == "The specified configuration does not exist.", $"unexpected message {Py.Repr(message)}");

        // --- success path: put config, read it back verbatim ---
        await s3.PutBucketMetricsConfigurationAsync(new PutBucketMetricsConfigurationRequest
        {
            BucketName = bucket,
            MetricsId = metricsId,
            MetricsConfiguration = new MetricsConfiguration
            {
                MetricsId = metricsId,
                MetricsFilter = new MetricsFilter { MetricsFilterPredicate = new MetricsPrefixPredicate("billing/") },
            },
        });
        var got = await s3.GetBucketMetricsConfigurationAsync(new GetBucketMetricsConfigurationRequest { BucketName = bucket, MetricsId = metricsId });
        var cfg = got.MetricsConfiguration;
        Ensure(cfg.MetricsId == metricsId, $"expected Id {Py.Repr(metricsId)}, got {Py.Repr(cfg.MetricsId)}");
        var prefix = (cfg.MetricsFilter?.MetricsFilterPredicate as MetricsPrefixPredicate)?.Prefix;
        Ensure(prefix == "billing/", $"expected Prefix 'billing/', got {Py.Repr(prefix)}");
    }

    /// <summary>
    /// The (Error.Code, Error.Message) of fn(); a code-less HEAD error yields the HTTP status. Raises
    /// AssertionError if fn succeeds.
    /// </summary>
    private static Task<(string? Code, string? Message)> ClientError(Func<Task> fn) =>
        S3Fidelity.ClientError(fn, "expected a ClientError, but the call SUCCEEDED");
}
