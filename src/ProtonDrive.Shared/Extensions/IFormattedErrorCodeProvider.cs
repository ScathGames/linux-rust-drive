using System.Diagnostics.CodeAnalysis;

namespace ProtonDrive.Shared.Extensions;

public interface IFormattedErrorCodeProvider
{
    bool TryGetRelevantFormattedErrorCode([MaybeNullWhen(false)] out string formattedErrorCode);
}
