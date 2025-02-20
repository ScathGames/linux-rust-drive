using System.Collections.Generic;

namespace ProtonDrive.App.Localization;

public interface ILanguageService
{
    string? CurrentLanguage { get; set; }
    IEnumerable<Language> GetSupportedLanguages();
}
