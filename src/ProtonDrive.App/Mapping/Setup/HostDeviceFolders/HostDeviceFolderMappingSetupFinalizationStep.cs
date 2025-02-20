using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProtonDrive.App.Settings;
using ProtonDrive.App.SystemIntegration;

namespace ProtonDrive.App.Mapping.Setup.HostDeviceFolders;

internal sealed class HostDeviceFolderMappingSetupFinalizationStep
{
    private readonly OnDemandSyncEligibilityValidator _onDemandSyncEligibilityValidator;
    private readonly IOnDemandSyncRootRegistry _onDemandSyncRootRegistry;
    private readonly ILogger<HostDeviceFolderMappingSetupFinalizationStep> _logger;

    public HostDeviceFolderMappingSetupFinalizationStep(
        OnDemandSyncEligibilityValidator onDemandSyncEligibilityValidator,
        IOnDemandSyncRootRegistry onDemandSyncRootRegistry,
        ILogger<HostDeviceFolderMappingSetupFinalizationStep> logger)
    {
        _onDemandSyncEligibilityValidator = onDemandSyncEligibilityValidator;
        _onDemandSyncRootRegistry = onDemandSyncRootRegistry;
        _logger = logger;
    }

    public async Task<MappingErrorCode> FinishSetupAsync(RemoteToLocalMapping mapping, CancellationToken cancellationToken)
    {
        if (mapping.Type is not MappingType.HostDeviceFolder)
        {
            throw new ArgumentException("Mapping type has unexpected value", nameof(mapping));
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Upon requesting to enable on-demand sync for the host device folder, the mapping sync method is still classic
        if (mapping.SyncMethod is not SyncMethod.OnDemand && !mapping.IsEnablingOnDemandSyncRequested())
        {
            return MappingErrorCode.None;
        }

        if (mapping.IsEnablingOnDemandSyncRequested() &&
            ValidateOnDemandSyncEligibility(mapping) is { } validationResult)
        {
            _logger.LogWarning("The local sync folder is not eligible for on-demand sync: {ErrorCode}", validationResult);

            mapping.SetEnablingOnDemandSyncFailed();
            return validationResult;
        }

        if (await AddOnDemandSyncRootAsync(mapping).ConfigureAwait(false) is { } errorCode)
        {
            mapping.SetEnablingOnDemandSyncFailed();
            return errorCode;
        }

        mapping.SetEnablingOnDemandSyncSucceeded();
        return MappingErrorCode.None;
    }

    private MappingErrorCode? ValidateOnDemandSyncEligibility(RemoteToLocalMapping mapping)
    {
        return _onDemandSyncEligibilityValidator.Validate(mapping.Local.Path);
    }

    private Task<MappingErrorCode?> AddOnDemandSyncRootAsync(RemoteToLocalMapping mapping)
    {
        return _onDemandSyncRootRegistry.TryAddOnDemandSyncRootAsync(mapping);
    }
}
