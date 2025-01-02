namespace ProtonDrive.App.Settings.Remote;

public interface IRemoteSettingsAware
{
    void OnRemoteSettingsChanged(RemoteSettings settings);
}
