using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ProtonDrive.App.Settings;
using ProtonDrive.Shared.Localization;
using ProtonDrive.Shared.Repository;

namespace ProtonDrive.App.Localization;

public sealed class LanguageService : ILanguageService, ILanguageProvider
{
    private readonly IRepository<LanguageSettings> _repository;

    private readonly HashSet<string> _supportedLanguages =
    [
        "en",
        "de",
        "es",
        "es-419",
        "fr",
        "it",
        "nl",
        "pl",
        "pt-BR",
        "tr",
        "ru",
    ];

    private readonly Dictionary<string, string> _regionalLanguages = new()
    {
        { "es-AR", "es-419" },
        { "es-BO", "es-419" },
        { "es-CL", "es-419" },
        { "es-CO", "es-419" },
        { "es-CR", "es-419" },
        { "es-CU", "es-419" },
        { "es-DO", "es-419" },
        { "es-EC", "es-419" },
        { "es-GT", "es-419" },
        { "es-HN", "es-419" },
        { "es-MX", "es-419" },
        { "es-NI", "es-419" },
        { "es-PA", "es-419" },
        { "es-PY", "es-419" },
        { "es-PE", "es-419" },
        { "es-PR", "es-419" },
        { "es-SV", "es-419" },
        { "es-UY", "es-419" },
        { "es-VE", "es-419" },
    };

    private readonly CultureInfo _systemUICulture;

    private string? _currentLanguage;

    public LanguageService(IRepository<LanguageSettings> repository)
    {
        _repository = repository;
        _systemUICulture = Thread.CurrentThread.CurrentUICulture;
        _currentLanguage = repository.Get()?.CultureName;
    }

    public string? CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            _repository.Set(value is null ? null : new LanguageSettings(cultureName: value));
        }
    }

    public IEnumerable<Language> GetSupportedLanguages()
    {
        var autoCulture = GetSupportedLanguageCulture(_systemUICulture);

        // TODO: Translate "Auto" if necessary
        var autoLanguage = new Language($"Auto ({GetCapitalizedCultureNativeName(autoCulture)})", null);

        var supportedLanguages = _supportedLanguages
            .Select(name => new Language(GetCapitalizedCultureNativeName(new CultureInfo(name)), name))
            .OrderBy(x => x.DisplayName)
            .Prepend(autoLanguage);

        return supportedLanguages;
    }

    public string GetCulture()
    {
        return CurrentLanguage ?? GetRegionalCulture() ?? _systemUICulture.Name;
    }

    private static string GetCapitalizedCultureNativeName(CultureInfo culture)
    {
        return Capitalize(culture.NativeName, culture);
    }

    private static string Capitalize(string input, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(input)
            ? string.Concat(char.ToUpper(input[0], culture).ToString(), input.AsSpan(1))
            : string.Empty;
    }

    private string? GetRegionalCulture()
    {
        return _regionalLanguages.TryGetValue(_systemUICulture.Name, out var cultureName) ? cultureName : null;
    }

    private CultureInfo GetSupportedLanguageCulture(CultureInfo requestedLanguageCulture)
    {
        if (_supportedLanguages.Contains(requestedLanguageCulture.Name))
        {
            return requestedLanguageCulture;
        }

        if (_regionalLanguages.TryGetValue(requestedLanguageCulture.Name, out var regionalCultureName) && _supportedLanguages.Contains(regionalCultureName))
        {
            return new CultureInfo(regionalCultureName);
        }

        return !requestedLanguageCulture.IsNeutralCulture
            ? GetSupportedLanguageCulture(requestedLanguageCulture.Parent)
            : new CultureInfo("en");
    }
}
