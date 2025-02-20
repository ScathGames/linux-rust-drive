﻿using System;
using ProtonDrive.App.Settings;
using ProtonDrive.Client.Contracts;

namespace ProtonDrive.App.Mapping.SyncFolders;

public sealed class SyncFolder
{
    private MappingState _state = MappingState.None;

    internal SyncFolder(RemoteToLocalMapping mapping)
    {
        Mapping = mapping;
        Type = ToSyncFolderType(mapping.Type);
        LocalPath = GetLocalPath();
        RootLinkType = mapping.Remote.RootItemType;
    }

    public LinkType RootLinkType { get; }
    public SyncFolderType Type { get; }
    public SyncMethod SyncMethod => GetSyncMethod();
    public string LocalPath { get; }
    public string? RemoteName => Mapping.Remote.RootItemName;
    public string? RemoteShareId => Mapping.Remote.ShareId;
    public bool RemoteIsReadOnly => Mapping.Remote.IsReadOnly;
    public MappingSetupStatus Status => _state.Status;
    public MappingErrorCode ErrorCode => _state.ErrorCode;

    internal RemoteToLocalMapping Mapping { get; }

    internal bool SetState(MappingState state)
    {
        var result = !state.Equals(_state);
        _state = state;

        return result;
    }

    private static SyncFolderType ToSyncFolderType(MappingType mappingType)
    {
        return mappingType switch
        {
            MappingType.CloudFiles => SyncFolderType.AccountRoot,
            MappingType.HostDeviceFolder => SyncFolderType.HostDeviceFolder,
            MappingType.ForeignDevice => SyncFolderType.ForeignDevice,
            MappingType.SharedWithMeRootFolder => SyncFolderType.SharedWithMeRoot,
            MappingType.SharedWithMeItem => SyncFolderType.SharedWithMeItem,
            _ => throw new ArgumentOutOfRangeException(nameof(mappingType)),
        };
    }

    private string GetLocalPath()
    {
        return Mapping.Type is MappingType.CloudFiles
            ? Mapping.TryGetAccountRootFolderPath(out var path) ? path : string.Empty
            : Mapping.Local.Path;
    }

    private SyncMethod GetSyncMethod()
    {
        return Mapping.SyncMethod is SyncMethod.OnDemand || Mapping.IsEnablingOnDemandSyncRequested()
            ? SyncMethod.OnDemand
            : Mapping.SyncMethod;
    }
}
