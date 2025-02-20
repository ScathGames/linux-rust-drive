using System;
using System.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using ProtonDrive.App.Settings;
using ProtonDrive.Shared.Repository;

namespace ProtonDrive.App.Windows.SystemIntegration;

internal sealed class WinRegistryLanguageRepository : IRepository<LanguageSettings>
{
    private const string ProtonDriveRegistryKeyName = @"Software\Proton\Drive";
    private const string LanguageValueName = "Language";

    private readonly ILogger<WinRegistryLanguageRepository> _logger;

    public WinRegistryLanguageRepository(ILogger<WinRegistryLanguageRepository> logger)
    {
        _logger = logger;
    }

    public LanguageSettings? Get()
    {
        try
        {
            using RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(ProtonDriveRegistryKeyName, writable: true);

            if (registryKey is null)
            {
                _logger.LogWarning("Failed to get the culture name: Registry {Key} not found", ProtonDriveRegistryKeyName);
                return null;
            }

            var languageValue = registryKey.GetValue(LanguageValueName);

            if (languageValue is not string value || string.IsNullOrEmpty(value))
            {
                return null;
            }

            return new LanguageSettings(value);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException or SecurityException or UnauthorizedAccessException)
        {
            _logger.LogWarning("Failed to get the culture name: {Message}", ex.Message);
            return null;
        }
    }

    public void Set(LanguageSettings? value)
    {
        try
        {
            using RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(ProtonDriveRegistryKeyName, writable: true);

            if (registryKey is null)
            {
                _logger.LogWarning("Failed to update app language: Registry {Key} not found", ProtonDriveRegistryKeyName);
                return;
            }

            if (value?.CultureName is null)
            {
                registryKey.DeleteValue(LanguageValueName);
                return;
            }

            registryKey.SetValue(LanguageValueName, value.CultureName);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException or SecurityException or UnauthorizedAccessException)
        {
            _logger.LogWarning("Failed to update app language: {Message}", ex.Message);
        }
    }
}
