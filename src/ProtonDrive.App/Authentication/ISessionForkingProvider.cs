namespace ProtonDrive.App.Authentication;

public interface ISessionForkingProvider
{
    string GetSessionForkSelector();
}
