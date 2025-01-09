using System;
using System.Diagnostics.CodeAnalysis;
using ProtonDrive.App.Settings;
using ProtonDrive.Shared.Extensions;

namespace ProtonDrive.App.Mapping;

public class MappingSetupException : Exception, IFormattedErrorCodeProvider
{
    public MappingSetupException()
    {
    }

    public MappingSetupException(string message)
        : base(message)
    {
    }

    public MappingSetupException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public MappingSetupException(MappingType mappingType, MappingErrorCode errorCode)
    {
        MappingType = mappingType;
        ErrorCode = errorCode;
    }

    public MappingType? MappingType { get; }
    public MappingErrorCode? ErrorCode { get; }

    public bool TryGetRelevantFormattedErrorCode([MaybeNullWhen(false)] out string formattedErrorCode)
    {
        formattedErrorCode = string.Empty;

        if (MappingType is null || ErrorCode is null)
        {
            return false;
        }

        formattedErrorCode = $"{MappingType}/{ErrorCode}";

        return true;
    }
}
