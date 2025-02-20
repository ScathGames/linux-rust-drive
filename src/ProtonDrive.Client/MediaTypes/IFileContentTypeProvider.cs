namespace ProtonDrive.Client.MediaTypes;

public interface IFileContentTypeProvider
{
    string GetContentType(string filename);
}
