namespace ProtonDrive.App.FileSystem.Remote;

internal static class RootPropertyProvider
{
    public static string GetVirtualRootFolderId(int mappingId) => "Virtual_" + mappingId;

    public static string GetEventScope(int volumeId) => "Volume_" + volumeId;
}
