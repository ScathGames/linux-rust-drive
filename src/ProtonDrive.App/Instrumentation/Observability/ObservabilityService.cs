using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProtonDrive.App.Settings.Remote;
using ProtonDrive.Client;
using ProtonDrive.Client.Instrumentation.Observability;
using ProtonDrive.Shared.Configuration;
using ProtonDrive.Shared.Extensions;
using ProtonDrive.Shared.Threading;

namespace ProtonDrive.App.Instrumentation.Observability;

internal sealed class ObservabilityService : IRemoteSettingsAware
{
    private readonly IObservabilityApiClient _observabilityApiClient;
    private readonly GenericFileTransferMetricsFactory _genericFileTransferMetricsFactory;
    private readonly ILogger<ObservabilityService> _logger;
    private readonly CancellationHandle _cancellationHandle = new();
    private readonly TimeSpan _period;

    private PeriodicTimer _timer;
    private Task? _timerTask;

    public ObservabilityService(
        AppConfig appConfig,
        IObservabilityApiClient observabilityApiClient,
        GenericFileTransferMetricsFactory genericFileTransferMetricsFactory,
        ILogger<ObservabilityService> logger)
    {
        _observabilityApiClient = observabilityApiClient;
        _genericFileTransferMetricsFactory = genericFileTransferMetricsFactory;
        _logger = logger;
        _period = appConfig.PeriodicObservabilityReportInterval.RandomizedWithDeviation(0.2);
        _timer = new PeriodicTimer(_period);
    }

    void IRemoteSettingsAware.OnRemoteSettingsChanged(RemoteSettings settings)
    {
        if (settings.IsTelemetryEnabled)
        {
            Start();
        }
        else
        {
            Stop();
        }
    }

    private void Start()
    {
        if (_timerTask is not null)
        {
            return;
        }

        _timer = new PeriodicTimer(_period);
        _timerTask = PeriodicallySendMetricsAsync(_cancellationHandle.Token);
    }

    private void Stop()
    {
        if (_timerTask is null)
        {
            return;
        }

        _cancellationHandle.Cancel();
        _timerTask = null;
        _timer.Dispose();
    }

    private async Task PeriodicallySendMetricsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                await SendMetricsAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            /* Do nothing */
        }
    }

    private async Task SendMetricsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var uploadMetrics = _genericFileTransferMetricsFactory.GetFileUploadMetrics();
            var downloadMetrics = _genericFileTransferMetricsFactory.GetFileDownloadMetrics();

            if (uploadMetrics.Count + downloadMetrics.Count == 0)
            {
                return;
            }

            var metrics = new ObservabilityMetrics(uploadMetrics.Concat(downloadMetrics).ToList());

            await _observabilityApiClient.SendMetricsAsync(metrics, cancellationToken).ThrowOnFailure().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to send observability metrics: {ErrorCode} {ErrorMessage}", ex.GetRelevantFormattedErrorCode(), ex.Message);
        }
    }
}
