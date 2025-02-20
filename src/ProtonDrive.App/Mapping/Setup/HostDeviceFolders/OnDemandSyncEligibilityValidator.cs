using System;
using System.IO;
using ProtonDrive.App.SystemIntegration;
using ProtonDrive.Shared.IO;

namespace ProtonDrive.App.Mapping.Setup.HostDeviceFolders;

internal sealed class OnDemandSyncEligibilityValidator
{
    private readonly ILocalVolumeInfoProvider _volumeInfoProvider;
    private readonly IKnownFolders _knownFolders;

    public OnDemandSyncEligibilityValidator(
        ILocalVolumeInfoProvider volumeInfoProvider,
        IKnownFolders knownFolders)
    {
        _volumeInfoProvider = volumeInfoProvider;
        _knownFolders = knownFolders;
    }

    public MappingErrorCode? Validate(string folderPath)
    {
        return
            ValidateDriveType(folderPath) ??
            ValidateFolderPath(folderPath);
    }

    private MappingErrorCode? ValidateDriveType(string path)
    {
        var driveType = _volumeInfoProvider.GetDriveType(path);

        // Folders on fixed disks are allowed to be synced on-demand, other types of volumes are prevented,
        // like removable storage devices, including USB flash drives. Note that external hard drives
        // connected through USB are reported as fixed disks.
        return driveType switch
        {
            DriveType.Fixed => null,
            DriveType.Unknown => MappingErrorCode.LocalFileSystemAccessFailed,
            _ => MappingErrorCode.LocalVolumeNotSupported,
        };
    }

    private MappingErrorCode? ValidateFolderPath(string path)
    {
        var desktopFolderPath = _knownFolders.GetPath(_knownFolders.Desktop);
        if (string.IsNullOrEmpty(desktopFolderPath))
        {
            return null;
        }

        path = PathComparison.EnsureTrailingSeparator(path);
        desktopFolderPath = PathComparison.EnsureTrailingSeparator(desktopFolderPath);

        // Desktop folder is prevented from being synced on-demand, but subfolders are allowed
        return path.Equals(desktopFolderPath, StringComparison.OrdinalIgnoreCase)
            ? MappingErrorCode.LocalFolderNonSyncableOnDemand
            : null;
    }
}
