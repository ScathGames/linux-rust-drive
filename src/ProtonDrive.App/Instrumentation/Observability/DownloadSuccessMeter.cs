using ProtonDrive.Sync.Shared.SyncActivity;

namespace ProtonDrive.App.Instrumentation.Observability;

internal sealed class DownloadSuccessMeter : SuccessMeterBase
{
    public DownloadSuccessMeter(AttemptRetryMonitors attemptRetryMonitors)
        : base(attemptRetryMonitors.DownloadAttemptRetryMonitor)
    {
    }

    public override bool CanProcessItem(SyncActivityItem<long> item)
    {
        return item.ActivityType is SyncActivityType.Download && item.Source is SyncActivitySource.OnDemandFileHydration;
    }
}
