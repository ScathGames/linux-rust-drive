using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonDrive.App.Mapping;
using ProtonDrive.App.Mapping.SyncFolders;
using ProtonDrive.App.Settings;
using ProtonDrive.App.SystemIntegration;
using ProtonDrive.App.Windows.Services;
using ProtonDrive.App.Windows.Views.Shared;
using ProtonDrive.Shared.Configuration;

namespace ProtonDrive.App.Windows.Views.Main.Computers;

internal class SyncedFolderViewModel : ObservableObject, IEquatable<SyncFolder>, IMappingStatusViewModel
{
    private readonly SyncFolder _syncFolder;
    private readonly ILocalFolderService _localFolderService;
    private readonly IDialogService _dialogService;
    private readonly FeatureFlags _featureFlags;
    private readonly Func<RemoveSyncFolderConfirmationViewModel> _removeFolderConfirmationViewModelFactory;

    private readonly Func<SyncFolder, CancellationToken, Task> _removeSyncFolderAsync;
    private readonly Func<SyncFolder, CancellationToken, Task> _enableOnDemandSyncAsync;

    private SyncMethod _syncMethod;
    private MappingSetupStatus _status;
    private MappingErrorCode _errorCode;

    public SyncedFolderViewModel(
        SyncFolder syncFolder,
        string name,
        ImageSource? icon,
        ILocalFolderService localFolderService,
        IDialogService dialogService,
        FeatureFlags featureFlags,
        Func<RemoveSyncFolderConfirmationViewModel> removeFolderConfirmationViewModelFactory,
        Func<SyncFolder, CancellationToken, Task> removeSyncFolderAsync,
        Func<SyncFolder, CancellationToken, Task> enableOnDemandSyncAsync)
    {
        _syncFolder = syncFolder;
        _localFolderService = localFolderService;
        _dialogService = dialogService;
        _featureFlags = featureFlags;
        _removeFolderConfirmationViewModelFactory = removeFolderConfirmationViewModelFactory;
        _removeSyncFolderAsync = removeSyncFolderAsync;
        _enableOnDemandSyncAsync = enableOnDemandSyncAsync;

        OpenFolderCommand = new AsyncRelayCommand(OpenFolder);
        RemoveFolderCommand = new AsyncRelayCommand(RemoveSyncFolderAsync);
        EnableOnDemandSyncCommand = new AsyncRelayCommand(EnableOnDemandSyncAsync, IsEnableOnDemandSyncEnabled);

        Name = name;
        Icon = icon;
        Update();
    }

    public string Path => _syncFolder.LocalPath;
    public string Name { get; }
    public ImageSource? Icon { get; }

    public ICommand OpenFolderCommand { get; }
    public ICommand RemoveFolderCommand { get; }
    public IRelayCommand EnableOnDemandSyncCommand { get; }

    public SyncMethod SyncMethod
    {
        get => _syncMethod;
        private set => SetProperty(ref _syncMethod, value);
    }

    public MappingSetupStatus Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public MappingErrorCode ErrorCode
    {
        get => _errorCode;
        private set => SetProperty(ref _errorCode, value);
    }

    public MappingErrorRenderingMode RenderingMode => MappingErrorRenderingMode.IconAndText;

    public void Update()
    {
        SyncMethod = _syncFolder.SyncMethod;
        Status = _syncFolder.Status;
        ErrorCode = _syncFolder.ErrorCode;

        EnableOnDemandSyncCommand.NotifyCanExecuteChanged();
    }

    public bool Equals(SyncFolder? other)
    {
        // ReSharper disable once PossibleUnintendedReferenceComparison
        return other is not null && _syncFolder == other;
    }

    private async Task OpenFolder()
    {
        await _localFolderService.OpenFolderAsync(Path).ConfigureAwait(true);
    }

    private Task RemoveSyncFolderAsync(CancellationToken cancellationToken)
    {
        var confirmationViewModel = _removeFolderConfirmationViewModelFactory.Invoke();

        confirmationViewModel.SetFolderName(Name);

        var confirmationResult = _dialogService.ShowConfirmationDialog(confirmationViewModel);

        if (confirmationResult != ConfirmationResult.Confirmed)
        {
            return Task.CompletedTask;
        }

        Status = MappingSetupStatus.SettingUp;

        return _removeSyncFolderAsync(_syncFolder, cancellationToken);
    }

    private bool IsEnableOnDemandSyncEnabled()
    {
        return
            _featureFlags.EnablingOnDemandSyncForHostDeviceFoldersEnabled &&
            Status is MappingSetupStatus.Succeeded &&
            SyncMethod is SyncMethod.Classic;
    }

    private Task EnableOnDemandSyncAsync(CancellationToken cancellationToken)
    {
        return _enableOnDemandSyncAsync(_syncFolder, cancellationToken);
    }
}
