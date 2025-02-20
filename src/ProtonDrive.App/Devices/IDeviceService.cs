using System.Threading;
using System.Threading.Tasks;

namespace ProtonDrive.App.Devices;

public interface IDeviceService
{
    Task SetUpDevicesAsync();
    Task<DeviceSetupResult> SetUpHostDeviceAsync(CancellationToken cancellationToken);
    Task RenameHostDeviceAsync(string name);
}
