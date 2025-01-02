using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProtonDrive.Client.Instrumentation.Observability;

namespace ProtonDrive.App.Instrumentation.Observability;

internal sealed class GenericUploadMetricsFactory
{
    private readonly IReadOnlyDictionary<AttemptRetryShareType, AttemptRetryMonitor<long>> _attemptRetryMonitor;

    public GenericUploadMetricsFactory(IReadOnlyDictionary<AttemptRetryShareType, AttemptRetryMonitor<long>> attemptRetryMonitor)
    {
        _attemptRetryMonitor = attemptRetryMonitor;
    }

    public ReadOnlyCollection<UploadSuccessRateMetric> GetMetrics()
    {
        var shareTypes = Enum.GetValues(typeof(AttemptRetryShareType));

        var uploadMetrics = new List<UploadSuccessRateMetric>(capacity: shareTypes.Length * 4);

        foreach (AttemptRetryShareType shareType in shareTypes)
        {
            var counters = _attemptRetryMonitor[shareType].GetAndResetCounters();

            if (counters.FirstAttemptSuccesses > 0)
            {
                uploadMetrics.Add(GetUploadMetric(counters.FirstAttemptSuccesses, isSuccess: true, isRetry: false, shareType));
            }

            if (counters.FirstAttemptFailures > 0)
            {
                uploadMetrics.Add(GetUploadMetric(counters.FirstAttemptFailures, isSuccess: false, isRetry: false, shareType));
            }

            if (counters.RetrySuccesses > 0)
            {
                uploadMetrics.Add(GetUploadMetric(counters.RetrySuccesses, isSuccess: true, isRetry: true, shareType));
            }

            if (counters.RetryFailures > 0)
            {
                uploadMetrics.Add(GetUploadMetric(counters.RetryFailures, isSuccess: false, isRetry: true, shareType));
            }
        }

        return uploadMetrics.AsReadOnly();
    }

    private static UploadSuccessRateMetric GetUploadMetric(int counter, bool isSuccess, bool isRetry, AttemptRetryShareType shareType)
    {
        var shareTypeLabel = shareType switch
        {
            AttemptRetryShareType.Main => "main",
            AttemptRetryShareType.Standard => "shared",
            AttemptRetryShareType.Device => "device",
            _ => throw new ArgumentOutOfRangeException(nameof(shareType), shareType, null),
        };

        var labels = new Dictionary<string, string>
        {
            { "status", isSuccess ? "success" : "failure" },
            { "retry", isRetry ? "true" : "false" },
            { "shareType", shareTypeLabel },
            { "initiator", "background" },
        };

        var properties = new ObservabilityMetricProperties(Value: counter, labels);
        return new UploadSuccessRateMetric(properties);
    }
}
