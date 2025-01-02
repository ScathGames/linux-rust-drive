namespace ProtonDrive.App.Settings.Remote;

public interface IRemoteSettingsStateAware
{
    void OnRemoteSettingsStateChanged(RemoteSettingsStatus status);
}
