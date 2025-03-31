namespace ProtonDrive.App.Mapping;

public enum StorageOptimizationErrorCode
{
    None,
    Unknown,
    VolumeNotSupported,
    RemovableVolumeNotSupported,
    NetworkVolumeNotSupported,
    ConflictingOnDemandSyncRootExists,
    ConflictingDescendantOnDemandSyncRootExists,
}
