using ProtonDrive.Sync.Shared.SyncActivity;

namespace ProtonDrive.App.Instrumentation.Observability;

internal sealed class UploadSuccessMeter : SuccessMeterBase
{
    public UploadSuccessMeter(AttemptRetryMonitors attemptRetryMonitors)
    : base(attemptRetryMonitors.UploadAttemptRetryMonitor)
    {
    }

    public override bool CanProcessItem(SyncActivityItem<long> item)
    {
        // Since the upload process hasn't been initiated,
        // we don't classify the inability to open or read the file (or other issues during this stage) as an "upload failure".
        if (item.Stage == SyncActivityStage.Preparation)
        {
            return false;
        }

        return item.ActivityType is SyncActivityType.Upload;
    }
}
