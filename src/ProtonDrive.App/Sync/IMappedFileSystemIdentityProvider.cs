using System.Threading;
using System.Threading.Tasks;
using ProtonDrive.Sync.Shared.Trees;

namespace ProtonDrive.App.Sync;

internal interface IMappedFileSystemIdentityProvider
{
    public Task<LooseCompoundAltIdentity<string>?> GetRemoteIdFromLocalIdOrDefaultAsync(
        LooseCompoundAltIdentity<long> localId,
        CancellationToken cancellationToken);
}
