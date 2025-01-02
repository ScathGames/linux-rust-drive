namespace ProtonDrive.App.SystemIntegration;

public interface IReadOnlyFileAttributeRemover
{
    bool TryRemoveFileReadOnlyAttributeInFolder(string folderPath);
    bool TryRemoveFileReadOnlyAttribute(string filePath);
}
