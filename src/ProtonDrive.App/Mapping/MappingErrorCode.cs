namespace ProtonDrive.App.Mapping;

public enum MappingErrorCode
{
    None,
    DriveAccessFailed,
    DriveVolumeDiverged,
    DriveShareDiverged,
    DriveHostDeviceDiverged,
    DriveFolderDoesNotExist,
    DriveFolderDiverged,
    RemoteShareWithMeItemDoesNotExist,
    RemoteShareWithMeItemPermissionsDiverged,
    LocalFileSystemAccessFailed,
    LocalVolumeNotSupported,
    LocalFolderDoesNotExist,
    LocalFolderDiverged,
    LocalAndRemoteFoldersNotEmpty,
    LocalFolderNotEmpty,
    LocalFolderIncludedByAnAlreadySyncedFolder,
    LocalFolderIncludesAnAlreadySyncedFolder,
    LocalFolderNonSyncable,
    OnDemandSyncRootNotRegistered,
    ConflictingOnDemandSyncRootExists,
    ConflictingDescendantOnDemandSyncRootExists,
    InsufficientLocalFreeSpace,
    InsufficientDeviceQuota,
    SharingDisabled,
}
