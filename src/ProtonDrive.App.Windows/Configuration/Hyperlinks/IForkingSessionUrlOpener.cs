using System.Threading;
using System.Threading.Tasks;

namespace ProtonDrive.App.Windows.Configuration.Hyperlinks;

internal interface IForkingSessionUrlOpener
{
    Task<bool> TryOpenUrlAsync(string url, string childClientId, CancellationToken cancellationToken);
}
