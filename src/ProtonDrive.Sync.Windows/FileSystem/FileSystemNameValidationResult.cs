namespace ProtonDrive.Sync.Windows.FileSystem;

public enum FileSystemNameValidationResult
{
    Valid,
    Empty,
    TooLong,
    ContainsInvalidCharacter,
    EndsWithSpace,
    EndsWithPeriod,
    Reserved,
}
