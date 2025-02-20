using ProtonDrive.App.Settings;

namespace ProtonDrive.App.Mapping;

internal static class RemoteToLocalMappingExtensions
{
    public static bool IsEnablingOnDemandSyncRequested(this RemoteToLocalMapping mapping)
    {
        return mapping.SyncMethodUpdateStatus is SyncMethodUpdateStatus.EnablingOnDemandSyncRequested;
    }

    public static void SetEnablingOnDemandSyncSucceeded(this RemoteToLocalMapping mapping)
    {
        if (mapping.SyncMethod is SyncMethod.OnDemand)
        {
            return;
        }

        mapping.SyncMethod = SyncMethod.OnDemand;
        mapping.SyncMethodUpdateStatus = SyncMethodUpdateStatus.None;
        mapping.IsDirty = true;
    }

    public static void SetEnablingOnDemandSyncFailed(this RemoteToLocalMapping mapping)
    {
        if (!mapping.IsEnablingOnDemandSyncRequested())
        {
            return;
        }

        mapping.SyncMethodUpdateStatus = SyncMethodUpdateStatus.EnablingOnDemandSyncFailed;
        mapping.IsDirty = true;
    }
}
