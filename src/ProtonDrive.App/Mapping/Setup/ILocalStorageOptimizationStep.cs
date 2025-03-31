using ProtonDrive.App.Settings;

namespace ProtonDrive.App.Mapping.Setup;

internal interface ILocalStorageOptimizationStep
{
    StorageOptimizationErrorCode? Execute(RemoteToLocalMapping mapping);
}
