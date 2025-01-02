using System;
using System.Collections.Generic;
using System.Linq;
using ProtonDrive.App.Mapping;
using ProtonDrive.App.Settings;
using ProtonDrive.App.Sync;
using ProtonDrive.Sync.Shared.FileSystem;
using ProtonDrive.Sync.Shared.SyncActivity;

namespace ProtonDrive.App.Instrumentation.Observability;

internal sealed class UploadSuccessMeter : ISyncActivityAware, IMappingsAware
{
    private readonly IReadOnlyDictionary<AttemptRetryShareType, AttemptRetryMonitor<long>> _attemptRetryMonitors;

    private IReadOnlyDictionary<int, MappingType> _mappingTypeById = new Dictionary<int, MappingType>();

    public UploadSuccessMeter(IReadOnlyDictionary<AttemptRetryShareType, AttemptRetryMonitor<long>> attemptRetryMonitors)
    {
        _attemptRetryMonitors = attemptRetryMonitors;
    }

    void ISyncActivityAware.OnSyncActivityChanged(SyncActivityItem<long> item)
    {
        if (item.ActivityType is not SyncActivityType.Upload)
        {
            return;
        }

        // Since the upload process hasn't been initiated,
        // we don't classify the inability to open or read the file (or other issues during this stage) as an "upload failure".
        if (item.Stage == SyncActivityStage.Preparation)
        {
            return;
        }

        switch (item.Status)
        {
            case SyncActivityItemStatus.Succeeded:
                var shareType = GetShareTypeByMappingId(mappingId: item.RootId);
                _attemptRetryMonitors[shareType].IncrementSuccess(item.Id);
                break;

            case SyncActivityItemStatus.Failed or SyncActivityItemStatus.Warning when !FailureShouldBeIgnored(item.ErrorCode):
                shareType = GetShareTypeByMappingId(mappingId: item.RootId);
                _attemptRetryMonitors[shareType].IncrementFailure(item.Id);
                break;
        }
    }

    void IMappingsAware.OnMappingsChanged(IReadOnlyCollection<RemoteToLocalMapping> activeMappings, IReadOnlyCollection<RemoteToLocalMapping> deletedMappings)
    {
        _mappingTypeById = activeMappings.Concat(deletedMappings)
            .Where(x => x.Type is not MappingType.SharedWithMeRootFolder)
            .ToDictionary(x => x.Id, x => x.Type)
            .AsReadOnly();
    }

    private static bool FailureShouldBeIgnored(FileSystemErrorCode errorCode)
    {
        return errorCode is FileSystemErrorCode.Cancelled
            or FileSystemErrorCode.TooManyChildren
            or FileSystemErrorCode.FreeSpaceExceeded
            or FileSystemErrorCode.NetworkError;
    }

    private static AttemptRetryShareType GetShareType(MappingType mappingType)
    {
        return mappingType switch
        {
            MappingType.CloudFiles => AttemptRetryShareType.Main,
            MappingType.ForeignDevice => AttemptRetryShareType.Device,
            MappingType.HostDeviceFolder => AttemptRetryShareType.Device,
            MappingType.SharedWithMeItem => AttemptRetryShareType.Standard,
            _ => throw new ArgumentOutOfRangeException(nameof(mappingType), mappingType, default),
        };
    }

    private AttemptRetryShareType GetShareTypeByMappingId(int? mappingId)
    {
        if (mappingId is null || !_mappingTypeById.TryGetValue(mappingId.Value, out var mappingType))
        {
            return AttemptRetryShareType.Main;
        }

        return GetShareType(mappingType);
    }
}
