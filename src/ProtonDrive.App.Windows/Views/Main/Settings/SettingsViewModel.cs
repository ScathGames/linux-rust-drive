using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ProtonDrive.App.Localization;
using ProtonDrive.App.Windows.SystemIntegration;
using ProtonDrive.Shared.Localization;

namespace ProtonDrive.App.Windows.Views.Main.Settings;

internal class SettingsViewModel : PageViewModel
{
    private readonly IOperatingSystemIntegrationService _operatingSystemIntegrationService;
    private readonly ILanguageService _languageService;
    private readonly string? _initialSelectedCulture;
    private readonly string _uiCulture;

    private bool _appIsOpeningOnStartup;
    private bool _languageHasChanged;
    private Language _selectedLanguage;

    public SettingsViewModel(
        IApp app,
        IOperatingSystemIntegrationService operatingSystemIntegrationService,
        AccountRootSyncFolderViewModel accountRotSyncFolder,
        ILanguageService languageService,
        ILanguageProvider languageProvider)
    {
        AccountRootSyncFolder = accountRotSyncFolder;
        _operatingSystemIntegrationService = operatingSystemIntegrationService;
        _languageService = languageService;
        _appIsOpeningOnStartup = _operatingSystemIntegrationService.GetRunApplicationOnStartup();

        SupportedLanguages = languageService.GetSupportedLanguages().ToList();
        _uiCulture = languageProvider.GetCulture();
        _selectedLanguage = GetSelectedLanguageOrDefault();
        _initialSelectedCulture = _selectedLanguage.CultureName;

        RestartAppCommand = new AsyncRelayCommand(app.RestartAsync);
    }

    public IReadOnlyList<Language> SupportedLanguages { get; }

    public Language SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value))
            {
                OnSelectedLanguageChanged(value);
            }
        }
    }

    public bool LanguageHasChanged
    {
        get => _languageHasChanged;
        set => SetProperty(ref _languageHasChanged, value);
    }

    public bool AppIsOpeningOnStartup
    {
        get => _appIsOpeningOnStartup;

        set
        {
            if (SetProperty(ref _appIsOpeningOnStartup, value))
            {
                _operatingSystemIntegrationService.SetRunApplicationOnStartup(value);
            }
        }
    }

    public ICommand RestartAppCommand { get; }

    public AccountRootSyncFolderViewModel AccountRootSyncFolder { get; }

    internal override void OnActivated()
    {
        AccountRootSyncFolder.ClearValidationResult();
    }

    private void OnSelectedLanguageChanged(Language value)
    {
        _languageService.CurrentLanguage = value.CultureName;
        LanguageHasChanged = !string.Equals(value.CultureName, _initialSelectedCulture ?? _uiCulture, StringComparison.OrdinalIgnoreCase);
    }

    private Language GetSelectedLanguageOrDefault()
    {
        if (_languageService.CurrentLanguage is null)
        {
            return GetDefaultLanguage();
        }

        return SupportedLanguages.FirstOrDefault(x => string.Equals(x.CultureName, _languageService.CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            ?? GetDefaultLanguage();

        Language GetDefaultLanguage()
        {
            return SupportedLanguages.First(x => x.CultureName is null);
        }
    }
}
