using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ProtonDrive.App.Account;
using ProtonDrive.App.Authentication;
using ProtonDrive.App.Features;
using ProtonDrive.App.Mapping;
using ProtonDrive.App.Mapping.SyncFolders;
using ProtonDrive.App.Notifications.Offers;
using ProtonDrive.App.Onboarding;
using ProtonDrive.App.Update;
using ProtonDrive.App.Windows.Configuration.Hyperlinks;
using ProtonDrive.App.Windows.Resources;
using ProtonDrive.App.Windows.Services;
using ProtonDrive.App.Windows.Views.BugReport;
using ProtonDrive.App.Windows.Views.Main.Account;
using ProtonDrive.App.Windows.Views.Offer;
using ProtonDrive.App.Windows.Views.Shared;
using ProtonDrive.App.Windows.Views.Shared.Navigation;
using ProtonDrive.App.Windows.Views.Shared.Notification;
using ProtonDrive.Shared.Features;
using ProtonDrive.Shared.Threading;

namespace ProtonDrive.App.Windows.Views.Main;

internal sealed class MainViewModel
    : ObservableObject,
        IApplicationPages, IUserStateAware, ISessionStateAware, IAccountStateAware, ISyncFoldersAware, IFeatureFlagsAware,
        IStorageOptimizationOnboardingStateAware, IOffersAware
{
    private readonly IApp _app;
    private readonly PageViewModelFactory _pageViewModelFactory;
    private readonly Func<BugReportViewModel> _bugReportViewModelFactory;
    private readonly Func<OfferViewModel> _offerViewModelFactory;
    private readonly IDialogService _dialogService;
    private readonly IExternalHyperlinks _externalHyperlinks;
    private readonly IUpgradeStoragePlanAvailabilityVerifier _upgradeStoragePlanAvailabilityVerifier;
    private readonly IScheduler _scheduler;

    private readonly NotificationBadge _newVersionNotificationBadge;
    private readonly NotificationBadge _syncFoldersFailureNotificationBadge;
    private readonly NotificationBadge _sharedWithMeFeatureDisabledNotificationBadge;
    private readonly NotificationBadge _storageOptimizationOnboardingNotificationBadge;
    private readonly NotificationBadge _updateRequiredNotificationBadge;
    private readonly NotificationBadge _warningLevel1QuotaNotificationBadge;
    private readonly NotificationBadge _warningLevel2QuotaNotificationBadge;
    private readonly NotificationBadge _exceededQuotaNotificationBadge;
    private readonly Dictionary<SyncFolderType, HashSet<string>> _failedSyncFoldersByType = [];

    private ApplicationPage _currentMenuItem;
    private IconStatus _accountIconStatus;
    private AccountDisplayStatus _accountDisplayStatus;
    private AccountErrorCode? _errorCode;
    private string? _offerTitle;
    private PageViewModel _page;
    private NotificationBadge? _settingsNotificationBadge;
    private NotificationBadge? _myComputerNotificationBadge;
    private NotificationBadge? _storageOptimizationOnboardingBadge;
    private NotificationBadge? _sharedWithMeNotificationBadge;
    private NotificationBadge? _updateNotificationBadge;
    private NotificationBadge? _quotaNotificationBadge;
    private UserState? _user;
    private SessionState _sessionState = SessionState.None;
    private AccountState _accountState = AccountState.None;
    private Notifications.Offers.Offer? _offer;
    private bool _sharingFeatureIsDisabled;
    private bool _isStorageOptimizationFeatureBadgeEnabled;
    private bool _isStorageOptimizationOnboarding;

    public MainViewModel(
        IApp app,
        INavigationService<DetailsPageViewModel> detailsPages,
        AppStateViewModel stateViewModel,
        PageViewModelFactory pageViewModelFactory,
        Func<BugReportViewModel> bugReportViewModelFactory,
        Func<OfferViewModel> offerViewModelFactory,
        IDialogService dialogService,
        IUpdateService updateService,
        IExternalHyperlinks externalHyperlinks,
        IUpgradeStoragePlanAvailabilityVerifier upgradeStoragePlanAvailabilityVerifier,
        [FromKeyedServices("Dispatcher")] IScheduler scheduler)
    {
        _app = app;
        _pageViewModelFactory = pageViewModelFactory;
        _bugReportViewModelFactory = bugReportViewModelFactory;
        _offerViewModelFactory = offerViewModelFactory;
        _dialogService = dialogService;
        _externalHyperlinks = externalHyperlinks;
        _upgradeStoragePlanAvailabilityVerifier = upgradeStoragePlanAvailabilityVerifier;
        _scheduler = scheduler;

        _updateRequiredNotificationBadge = new NotificationBadge(
            "!",
            Strings.Main_Sidebar_Notification_UpdateRequired_Description,
            NotificationBadgeSeverity.Alert);

        _newVersionNotificationBadge = new NotificationBadge(
            "!",
            Strings.Main_Sidebar_Notification_NewVersion_Description,
            NotificationBadgeSeverity.Warning);

        _syncFoldersFailureNotificationBadge = new NotificationBadge(
            "!",
            Strings.Main_Sidebar_Notification_SyncFoldersFailure_Description,
            NotificationBadgeSeverity.Warning);

        _sharedWithMeFeatureDisabledNotificationBadge = new NotificationBadge(
            "!",
            Strings.Main_Sidebar_Notification_SharingUnavailable_Description,
            NotificationBadgeSeverity.Warning);

        _exceededQuotaNotificationBadge = new NotificationBadge(
            "!",
            Strings.Main_Sidebar_Notification_ExceededQuota_Description,
            NotificationBadgeSeverity.Alert);

        _warningLevel2QuotaNotificationBadge = new NotificationBadge(
            "!",
            Strings.Main_Sidebar_Notification_ExceedingQuota_Description,
            NotificationBadgeSeverity.Alert);

        _warningLevel1QuotaNotificationBadge = new NotificationBadge(
            "!",
            Strings.Main_Sidebar_Notification_ExceedingQuota_Description,
            NotificationBadgeSeverity.Warning);

        _storageOptimizationOnboardingNotificationBadge = new NotificationBadge(
            "New",
            Strings.Main_Sidebar_Notification_GetStarted_Description,
            NotificationBadgeSeverity.Info);

        AppState = stateViewModel;
        DetailsPages = detailsPages;
        OpenAccountPageCommand = new RelayCommand(() => CurrentMenuItem = ApplicationPage.Account);
        updateService.StateChanged += OnUpdateServiceStateChanged;

        ReportBugCommand = new RelayCommand(ReportBug);
        GetMoreStorageCommand = new RelayCommand(GetMoreStorage, CanGetMoreStorage);
        OpenOfferCommand = new RelayCommand(OpenOffer, CanOpenOffer);
        OpenWebDashboardCommand = new RelayCommand(OpenWebDashboard);

        _page = ToPageViewModel(CurrentMenuItem);
    }

    public INavigationService<DetailsPageViewModel> DetailsPages { get; }

    public ApplicationPage CurrentMenuItem
    {
        get => _currentMenuItem;
        set
        {
            if (SetProperty(ref _currentMenuItem, value))
            {
                Page = ToPageViewModel(value);
            }
        }
    }

    public ICommand OpenAccountPageCommand { get; }
    public ICommand OpenWebDashboardCommand { get; }
    public IRelayCommand GetMoreStorageCommand { get; }
    public IRelayCommand OpenOfferCommand { get; }
    public ICommand ReportBugCommand { get; }

    public PageViewModel Page
    {
        get => _page;
        private set
        {
            if (SetProperty(ref _page, value))
            {
                value.OnActivated();
            }
        }
    }

    public UserState? User
    {
        get => _user;
        set
        {
            if (SetProperty(ref _user, value))
            {
                Schedule(RefreshCommandsAndStatuses);
            }
        }
    }

    public AppStateViewModel AppState { get; }

    public NotificationBadge? MyComputerNotificationBadge
    {
        get => _myComputerNotificationBadge;
        private set => SetProperty(ref _myComputerNotificationBadge, value);
    }

    public NotificationBadge? StorageOptimizationOnboardingBadge
    {
        get => _storageOptimizationOnboardingBadge;
        private set => SetProperty(ref _storageOptimizationOnboardingBadge, value);
    }

    public NotificationBadge? SharedWithMeNotificationBadge
    {
        get => _sharedWithMeNotificationBadge;
        private set => SetProperty(ref _sharedWithMeNotificationBadge, value);
    }

    public NotificationBadge? SettingsNotificationBadge
    {
        get => _settingsNotificationBadge;
        private set => SetProperty(ref _settingsNotificationBadge, value);
    }

    public NotificationBadge? UpdateNotificationBadge
    {
        get => _updateNotificationBadge;
        private set => SetProperty(ref _updateNotificationBadge, value);
    }

    public NotificationBadge? QuotaNotificationBadge
    {
        get => _quotaNotificationBadge;
        private set => SetProperty(ref _quotaNotificationBadge, value);
    }

    public IconStatus AccountIconStatus
    {
        get => _accountIconStatus;
        private set => SetProperty(ref _accountIconStatus, value);
    }

    public AccountDisplayStatus AccountDisplayStatus
    {
        get => _accountDisplayStatus;
        private set => SetProperty(ref _accountDisplayStatus, value);
    }

    public AccountErrorCode? ErrorCode
    {
        get => _errorCode;
        private set => SetProperty(ref _errorCode, value);
    }

    public string? OfferTitle
    {
        get => _offerTitle;
        private set => SetProperty(ref _offerTitle, value);
    }

    public void Show(ApplicationPage page)
    {
        Schedule(
            () =>
            {
                _app.ActivateAsync();
                ShowPage(page);
            });
    }

    public void OnUserStateChanged(UserState value)
    {
        User = value.IsEmpty ? null : value;
        UpdateQuotaNotification();
    }

    public void OnSessionStateChanged(SessionState value)
    {
        _sessionState = value;
        Schedule(RefreshCommandsAndStatuses);
    }

    public void OnAccountStateChanged(AccountState value)
    {
        _accountState = value;
        Schedule(RefreshCommandsAndStatuses);
    }

    void ISyncFoldersAware.OnSyncFolderChanged(SyncFolderChangeType changeType, SyncFolder folder)
    {
        Schedule(() => RefreshSyncFolderNotificationBadges(changeType, folder));
    }

    void IFeatureFlagsAware.OnFeatureFlagsChanged(IReadOnlyCollection<(Feature Feature, bool IsEnabled)> features)
    {
        _isStorageOptimizationFeatureBadgeEnabled =
            features.IsEnabled(Feature.DriveWindowsStorageOptimizationNewFeatureBadge) &&
            !features.IsEnabled(Feature.DriveWindowsStorageOptimizationDisabled);

        Schedule(() => RefreshSharedWithMeNotificationBadge(features));
        Schedule(RefreshStorageOptimizationOnboardingBadge);
    }

    void IStorageOptimizationOnboardingStateAware.StorageOptimizationOnboardingStateChanged(StorageOptimizationOnboardingStep step)
    {
        _isStorageOptimizationOnboarding = step is not StorageOptimizationOnboardingStep.None;
        Schedule(RefreshStorageOptimizationOnboardingBadge);
    }

    void IOffersAware.OnActiveOfferChanged(Notifications.Offers.Offer? offer)
    {
        _offer = offer;
        OfferTitle = offer?.Title;

        Schedule(
            () =>
            {
                GetMoreStorageCommand.NotifyCanExecuteChanged();
                OpenOfferCommand.NotifyCanExecuteChanged();
            });
    }

    private void RefreshSharedWithMeNotificationBadge(IReadOnlyCollection<(Feature Feature, bool IsEnabled)> features)
    {
        _sharingFeatureIsDisabled = features.IsEnabled(Feature.DriveSharingDisabled) || features.IsEnabled(Feature.DriveSharingEditingDisabled);
        SharedWithMeNotificationBadge = GetSharedWithMeNotificationBadge();
    }

    private void RefreshStorageOptimizationOnboardingBadge()
    {
        StorageOptimizationOnboardingBadge = _isStorageOptimizationFeatureBadgeEnabled && _isStorageOptimizationOnboarding
            ? _storageOptimizationOnboardingNotificationBadge
            : null;
    }

    private NotificationBadge? GetSharedWithMeNotificationBadge()
    {
        if (_sharingFeatureIsDisabled)
        {
            return _sharedWithMeFeatureDisabledNotificationBadge;
        }

        // If the sharing feature happens to be reactivated
        // but some mappings failed to set up, the notification badge is kept but updated.
        return _failedSyncFoldersByType.TryGetValue(SyncFolderType.SharedWithMeItem, out var failures) && failures.Count > 0
            ? _syncFoldersFailureNotificationBadge
            : null;
    }

    private void RefreshSyncFolderNotificationBadges(SyncFolderChangeType changeType, SyncFolder folder)
    {
        if (folder.Type is not (
            SyncFolderType.HostDeviceFolder
            or SyncFolderType.AccountRoot
            or SyncFolderType.SharedWithMeItem))
        {
            return;
        }

        var folderFailedToSetup = changeType is not SyncFolderChangeType.Removed
            && folder.Status is MappingSetupStatus.Failed or MappingSetupStatus.PartiallySucceeded;

        if (!_failedSyncFoldersByType.TryGetValue(folder.Type, out var failedFolders))
        {
            if (!folderFailedToSetup)
            {
                return;
            }

            failedFolders = [];
            _failedSyncFoldersByType.Add(folder.Type, failedFolders);
        }

        if (folderFailedToSetup)
        {
            failedFolders.Add(folder.LocalPath);
            SetNotificationBadgeForFolderType(folder.Type, isVisible: true);
        }
        else
        {
            failedFolders.Remove(folder.LocalPath);
            SetNotificationBadgeForFolderType(folder.Type, isVisible: failedFolders.Count > 0);
        }

        return;

        void SetNotificationBadgeForFolderType(SyncFolderType folderType, bool isVisible)
        {
            switch (folderType)
            {
                case SyncFolderType.HostDeviceFolder:
                    MyComputerNotificationBadge = isVisible ? _syncFoldersFailureNotificationBadge : null;
                    break;

                case SyncFolderType.AccountRoot:
                    SettingsNotificationBadge = isVisible ? _syncFoldersFailureNotificationBadge : null;
                    break;

                case SyncFolderType.SharedWithMeItem:
                    SharedWithMeNotificationBadge = GetSharedWithMeNotificationBadge();
                    break;
            }
        }
    }

    private void ShowPage(ApplicationPage page)
    {
        CurrentMenuItem = page;
    }

    private void UpdateQuotaNotification()
    {
        QuotaNotificationBadge = _user?.UserQuotaStatus switch
        {
            UserQuotaStatus.LimitExceeded => _exceededQuotaNotificationBadge,
            UserQuotaStatus.WarningLevel2Exceeded => _warningLevel2QuotaNotificationBadge,
            UserQuotaStatus.WarningLevel1Exceeded => _warningLevel1QuotaNotificationBadge,
            _ => null,
        };
    }

    private PageViewModel ToPageViewModel(ApplicationPage page)
    {
        return _pageViewModelFactory.Create(page) ?? _page;
    }

    private void OnUpdateServiceStateChanged(object? sender, UpdateState state)
    {
        if (state.UpdateRequired)
        {
            UpdateNotificationBadge = _updateRequiredNotificationBadge;
        }
        else if (state.IsReady)
        {
            UpdateNotificationBadge = _newVersionNotificationBadge;
        }
        else
        {
            UpdateNotificationBadge = null;
        }
    }

    private void RefreshCommandsAndStatuses()
    {
        AccountIconStatus = GetAccountIconStatus();
        AccountDisplayStatus = GetAccountDisplayStatus();
        ErrorCode = _accountState.ErrorCode;

        GetMoreStorageCommand.NotifyCanExecuteChanged();
    }

    private AccountDisplayStatus GetAccountDisplayStatus()
    {
        return _sessionState.Status switch
        {
            SessionStatus.NotStarted => AccountDisplayStatus.SignedOut,
            SessionStatus.Starting => AccountDisplayStatus.SigningIn,
            SessionStatus.SigningIn => AccountDisplayStatus.SigningIn,
            SessionStatus.Started => _accountState.Status switch
            {
                AccountStatus.None => AccountDisplayStatus.SettingUp,
                AccountStatus.SettingUp => AccountDisplayStatus.SettingUp,
                AccountStatus.Succeeded => AccountDisplayStatus.Succeeded,
                AccountStatus.Failed => AccountDisplayStatus.AccountError,
                _ => throw new ArgumentOutOfRangeException(),
            },
            SessionStatus.Ending => AccountDisplayStatus.SigningOut,
            SessionStatus.Failed => AccountDisplayStatus.SessionError,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private IconStatus GetAccountIconStatus()
    {
        return _sessionState.Status switch
        {
            SessionStatus.NotStarted => IconStatus.None,
            SessionStatus.Starting => IconStatus.None,
            SessionStatus.SigningIn => IconStatus.None,
            SessionStatus.Started => _accountState.Status switch
            {
                AccountStatus.None => IconStatus.None,
                AccountStatus.SettingUp => IconStatus.None,
                AccountStatus.Succeeded => IconStatus.Success,
                AccountStatus.Failed => IconStatus.Error,
                _ => throw new ArgumentOutOfRangeException(),
            },
            SessionStatus.Ending => IconStatus.None,
            SessionStatus.Failed => IconStatus.Error,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private void ReportBug()
    {
        var dialog = _bugReportViewModelFactory.Invoke();
        _dialogService.Show(dialog);
    }

    private bool CanGetMoreStorage()
    {
        return !CanOpenOffer() && _upgradeStoragePlanAvailabilityVerifier.UpgradedPlanIsAvailable(UpgradeStoragePlanMode.Sidebar, _user?.SubscriptionPlanCode);
    }

    private void GetMoreStorage()
    {
        _externalHyperlinks.UpgradePlanFromSidebar.Open();
    }

    private bool CanOpenOffer()
    {
        return _offer is not null;
    }

    private void OpenOffer()
    {
        var offer = _offer;
        if (offer is null)
        {
            return;
        }

        var dialog = _offerViewModelFactory.Invoke();
        if (!dialog.SetDataItem(offer))
        {
            return;
        }

        _dialogService.ShowDialog(dialog);
    }

    private void OpenWebDashboard()
    {
        _externalHyperlinks.Dashboard.Open();
    }

    private void Schedule(Action action)
    {
        _scheduler.Schedule(action);
    }
}
