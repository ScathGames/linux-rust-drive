namespace ProtonDrive.Sync.Shared.FileSystem;

public interface IFileSystemErrorCodeProvider
{
    public FileSystemErrorCode ErrorCode { get; }
}
