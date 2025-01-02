namespace ProtonDrive.App.SystemIntegration;

public interface IPlaceholderToRegularItemConverter
{
    bool TryConvertToRegularFolder(string path, bool skipRoot);
    bool TryConvertToRegularFile(string path);
}
