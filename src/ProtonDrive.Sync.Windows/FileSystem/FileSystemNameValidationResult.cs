namespace ProtonDrive.Sync.Windows.FileSystem;

public enum FileSystemNameValidationResult
{
    Success,
    Empty,
    TooLong,
    InvalidCharacters,
    EndsWithSpace,
    EndsWithPeriod,
    Reserved,
}
