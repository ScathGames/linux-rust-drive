using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtonDrive.App.Account;
using ProtonDrive.App.Authentication;
using ProtonDrive.App.Configuration;
using ProtonDrive.App.Devices;
using ProtonDrive.App.Features;
using ProtonDrive.App.InterProcessCommunication;
using ProtonDrive.App.Mapping;
using ProtonDrive.App.Mapping.SyncFolders;
using ProtonDrive.App.Notifications;
using ProtonDrive.App.Notifications.Offers;
using ProtonDrive.App.Onboarding;
using ProtonDrive.App.Reporting;
using ProtonDrive.App.Services;
using ProtonDrive.App.Sync;
using ProtonDrive.App.SystemIntegration;
using ProtonDrive.App.Volumes;
using ProtonDrive.App.Windows.Configuration.Hyperlinks;
using ProtonDrive.App.Windows.InterProcessCommunication;
using ProtonDrive.App.Windows.Services;
using ProtonDrive.App.Windows.SystemIntegration;
using ProtonDrive.App.Windows.Toolkit.Threading;
using ProtonDrive.App.Windows.Views.BugReport;
using ProtonDrive.App.Windows.Views.Main;
using ProtonDrive.App.Windows.Views.Main.About;
using ProtonDrive.App.Windows.Views.Main.Account;
using ProtonDrive.App.Windows.Views.Main.Activity;
using ProtonDrive.App.Windows.Views.Main.Computers;
using ProtonDrive.App.Windows.Views.Main.Settings;
using ProtonDrive.App.Windows.Views.Main.SharedWithMe;
using ProtonDrive.App.Windows.Views.Offer;
using ProtonDrive.App.Windows.Views.Onboarding;
using ProtonDrive.App.Windows.Views.Shared;
using ProtonDrive.App.Windows.Views.Shared.Navigation;
using ProtonDrive.App.Windows.Views.SignIn;
using ProtonDrive.App.Windows.Views.SystemTray;
using ProtonDrive.Shared;
using ProtonDrive.Shared.Offline;
using ProtonDrive.Shared.Threading;
using ProtonDrive.Sync.Shared.FileSystem;
using ProtonDrive.Sync.Windows.FileSystem.Client;

namespace ProtonDrive.App.Windows.Configuration;

internal static class AppServices
{
    public static IHostBuilder AddApp(this IHostBuilder builder, AppArguments appArguments)
    {
        return builder.ConfigureServices(
            services =>
                services
                    .AddSingleton(appArguments)
                    .AddSingleton<AppLifecycleService>()
                    .AddSingleton<App>()
                    .AddSingleton<IApp>(provider => provider.GetRequiredService<App>())
                    .AddSingleton<ISessionStateAware>(provider => provider.GetRequiredService<App>())
                    .AddSingleton<IOnboardingStateAware>(provider => provider.GetRequiredService<App>())
                    .AddSingleton<IDialogService>(provider => provider.GetRequiredService<App>()));
    }

    public static IHostBuilder AddServices(this IHostBuilder builder, IErrorReporting errorReporting, AppLaunchMode launchMode)
    {
        return builder.ConfigureServices(services => AddWindowsAppServices(services, errorReporting, launchMode));
    }

    public static void InitializeServices(this IServiceProvider provider)
    {
        provider.InitializeAppServices();
    }

    private static void AddWindowsAppServices(IServiceCollection services, IErrorReporting errorReporting, AppLaunchMode launchMode)
    {
        services
            .AddAppServices(errorReporting, launchMode)

            .AddSingleton(new DispatcherScheduler(Dispatcher.CurrentDispatcher))
            .AddKeyedSingleton<IScheduler>("Dispatcher", (sp, _) => sp.GetRequiredService<DispatcherScheduler>())
            .AddSingleton<IFileSystemDisplayNameAndIconProvider, Win32FileSystemDisplayNameAndIconProvider>()
            .AddSingleton<IFileSystemItemTypeProvider>(_ => new CachingFileSystemItemTypeProvider(new Win32FileSystemItemTypeProvider()))
            .AddSingleton<IOperatingSystemIntegrationService, OperatingSystemIntegrationService>()
            .AddSingleton<ILocalVolumeInfoProvider, VolumeInfoProvider>()
            .AddSingleton<ILocalFolderService, LocalFolderService>()
            .AddSingleton<IReadOnlyFileAttributeRemover, ReadOnlyFileAttributeRemover>()
            .AddSingleton<IPlaceholderToRegularItemConverter, PlaceholderToRegularItemConverter>()
            .AddSingleton<INonSyncablePathProvider, NonSyncablePathProvider>()
            .AddSingleton<INotificationService, SystemToastNotificationService>()
            .AddSingleton<IUrlOpener, UrlOpener>()
            .AddSingleton<IForkingSessionUrlOpener, ForkingSessionUrlOpener>()
            .AddSingleton<IExternalHyperlinks, ExternalHyperlinks>()
            .AddSingleton<IClipboard, SystemClipboard>()
            .AddSingleton<ISyncFolderStructureProtector>(
                provider =>
                    new SafeSyncFolderStructureProtectorDecorator(
                        new LoggingSyncFolderStructureProtectorDecorator(
                            provider.GetRequiredService<ILogger<LoggingSyncFolderStructureProtectorDecorator>>(),
                            new NtfsPermissionsBasedSyncFolderStructureProtector())))
            .AddSingleton<IShellSyncFolderRegistry, Win32ShellSyncFolderRegistry>()

            .AddSingleton<CloudFilterSyncRootRegistry>()
            .AddSingleton<IOnDemandSyncRootRegistry>(provider => provider.GetRequiredService<CloudFilterSyncRootRegistry>())
            .AddSingleton<ISessionStateAware>(provider => provider.GetRequiredService<CloudFilterSyncRootRegistry>())

            .AddSingleton<IFolderAppearanceCustomizer, Win32FolderAppearanceCustomizer>()

            .AddSingleton<UpdateNotificationService>()
            .AddSingleton<IStartableService>(provider => provider.GetRequiredService<UpdateNotificationService>())

            .AddSingleton<OfferNotificationService>()
            .AddSingleton<IStoppableService>(provider => provider.GetRequiredService<OfferNotificationService>())
            .AddSingleton<IOnboardingStateAware>(provider => provider.GetRequiredService<OfferNotificationService>())
            .AddSingleton<IOffersAware>(provider => provider.GetRequiredService<OfferNotificationService>())

            .AddSingleton<AppCommands>()
            .AddSingleton<ISessionStateAware>(provider => provider.GetRequiredService<AppCommands>())
            .AddSingleton<ISyncFoldersAware>(provider => provider.GetRequiredService<AppCommands>())
            .AddSingleton<AppStateViewModel>()
            .AddSingleton<ISessionStateAware>(provider => provider.GetRequiredService<AppStateViewModel>())
            .AddSingleton<IVolumeStateAware>(provider => provider.GetRequiredService<AppStateViewModel>())
            .AddSingleton<IAccountStateAware>(provider => provider.GetRequiredService<AppStateViewModel>())
            .AddSingleton<IMappingsSetupStateAware>(provider => provider.GetRequiredService<AppStateViewModel>())
            .AddSingleton<ISyncStateAware>(provider => provider.GetRequiredService<AppStateViewModel>())
            .AddSingleton<IUserStateAware>(provider => provider.GetRequiredService<AppStateViewModel>())
            .AddSingleton<IOfflineStateAware>(provider => provider.GetRequiredService<AppStateViewModel>())
            .AddSingleton<SessionWorkflowViewModel>()
            .AddSingleton<ISessionStateAware>(provider => provider.GetRequiredService<SessionWorkflowViewModel>())
            .AddSingleton<NavigationService<DetailsPageViewModel>>()
            .AddSingleton<INavigationService<DetailsPageViewModel>>(provider => provider.GetRequiredService<NavigationService<DetailsPageViewModel>>())
            .AddSingleton<INavigatablePages<DetailsPageViewModel>>(provider => provider.GetRequiredService<NavigationService<DetailsPageViewModel>>())
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<PageViewModelFactory>()
            .AddSingleton<IUpgradeStoragePlanAvailabilityVerifier, UpgradeStoragePlanAvailabilityVerifier>()

            .AddSingleton<OnboardingViewModel>()
            .AddSingleton<IOnboardingStateAware>(provider => provider.GetRequiredService<OnboardingViewModel>())
            .AddSingleton<SyncFolderSelectionStepViewModel>()
            .AddSingleton<AccountRootFolderSelectionStepViewModel>()
            .AddSingleton<ISessionStateAware>(provider => provider.GetRequiredService<AccountRootFolderSelectionStepViewModel>())
            .AddSingleton<UpgradeStorageStepViewModel>()
            .AddSingleton<IUserStateAware>(provider => provider.GetRequiredService<UpgradeStorageStepViewModel>())

            .AddSingleton<MainViewModel>()
            .AddSingleton<IApplicationPages>(provider => provider.GetRequiredService<MainViewModel>())
            .AddSingleton<IAccountStateAware>(provider => provider.GetRequiredService<MainViewModel>())
            .AddSingleton<IUserStateAware>(provider => provider.GetRequiredService<MainViewModel>())
            .AddSingleton<ISyncFoldersAware>(provider => provider.GetRequiredService<MainViewModel>())
            .AddSingleton<IFeatureFlagsAware>(provider => provider.GetRequiredService<MainViewModel>())
            .AddSingleton<ISharedWithMeOnboardingStateAware>(provider => provider.GetRequiredService<MainViewModel>())
            .AddSingleton<IOffersAware>(provider => provider.GetRequiredService<MainViewModel>())

            .AddTransient<AddedFolderValidationResultMessageBuilder>()
            .AddTransient<AddFoldersViewModel>()
            .AddTransient<Func<AddFoldersViewModel>>(provider => provider.GetRequiredService<AddFoldersViewModel>)

            .AddSingleton<SyncedDevicesViewModel>()
            .AddSingleton<IDeviceServiceStateAware>(provider => provider.GetRequiredService<SyncedDevicesViewModel>())
            .AddSingleton<IDevicesAware>(provider => provider.GetRequiredService<SyncedDevicesViewModel>())
            .AddSingleton<ISyncFoldersAware>(provider => provider.GetRequiredService<SyncedDevicesViewModel>())
            .AddSingleton<IMappingStateAware>(provider => provider.GetRequiredService<SyncedDevicesViewModel>())

            .AddSingleton<SharedWithMeViewModel>()
            .AddSingleton<ISharedWithMeOnboardingStateAware>(provider => provider.GetRequiredService<SharedWithMeViewModel>())

            .AddSingleton<SharedWithMeListViewModel>()
            .AddSingleton<ISyncFoldersAware>(provider => provider.GetRequiredService<SharedWithMeListViewModel>())
            .AddSingleton<IFeatureFlagsAware>(provider => provider.GetRequiredService<SharedWithMeListViewModel>())
            .AddSingleton<ISyncStateAware>(provider => provider.GetRequiredService<SharedWithMeListViewModel>())
            .AddSingleton<SharedWithMeItemViewModelFactory>()

            .AddSingleton<SettingsViewModel>()
            .AddSingleton<AboutViewModel>()
            .AddTransient<BugReportViewModel>()
            .AddTransient<Func<BugReportViewModel>>(provider => provider.GetRequiredService<BugReportViewModel>)
            .AddSingleton<AccountViewModel>()
            .AddSingleton<IUserStateAware>(provider => provider.GetRequiredService<AccountViewModel>())
            .AddSingleton<ISessionStateAware>(provider => provider.GetRequiredService<AccountViewModel>())
            .AddSingleton<AccountRootSyncFolderViewModel>()
            .AddSingleton<ISessionStateAware>(provider => provider.GetRequiredService<AccountRootSyncFolderViewModel>())
            .AddSingleton<ISyncFoldersAware>(provider => provider.GetRequiredService<AccountRootSyncFolderViewModel>())
            .AddSingleton<SyncStateViewModel>()
            .AddSingleton<ISessionStateAware>(provider => provider.GetRequiredService<SyncStateViewModel>())
            .AddSingleton<ISyncStateAware>(provider => provider.GetRequiredService<SyncStateViewModel>())
            .AddSingleton<ISyncActivityAware>(provider => provider.GetRequiredService<SyncStateViewModel>())
            .AddSingleton<SystemTrayViewModel>()

            .AddTransient<OfferViewModel>()
            .AddTransient<Func<OfferViewModel>>(provider => provider.GetRequiredService<OfferViewModel>)

            .AddSingleton(
                provider => new NamedPipeBasedIpcServer(
                    NamedPipeBasedIpcServer.PipeName,
                    provider.GetRequiredService<Lazy<IEnumerable<IIpcMessageHandler>>>(),
                    provider.GetRequiredService<ILogger<NamedPipeBasedIpcServer>>()))
            .AddSingleton<IStartableService>(provider => provider.GetRequiredService<NamedPipeBasedIpcServer>())
            .AddSingleton<IStoppableService>(provider => provider.GetRequiredService<NamedPipeBasedIpcServer>())

            .AddSingleton<IThumbnailGenerator, Win32ThumbnailGenerator>()
            .AddSingleton<IFileSystemClient<long>>(provider => new ClassicFileSystemClient(provider.GetRequiredService<IThumbnailGenerator>()))

            .AddSingleton<IFileSystemIdentityProvider<long>, FileSystemIdentityProvider>()
            ;
    }
}
