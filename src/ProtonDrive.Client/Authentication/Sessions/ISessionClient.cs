using System.Threading;
using System.Threading.Tasks;

namespace ProtonDrive.Client.Authentication.Sessions;

public interface ISessionClient
{
    Task<string> ForkSessionAsync(SessionForkingParameters parameters, CancellationToken cancellationToken);
}
