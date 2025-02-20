namespace ProtonDrive.App.SystemIntegration;

public enum OnDemandSyncRootVerificationVerdict
{
    /// <summary>
    /// Provided folder path does not belong to on-demand sync root
    /// </summary>
    NotRegistered,

    /// <summary>
    /// Provided folder path belongs to on-demand sync root with expected characteristics
    /// </summary>
    Valid,

    /// <summary>
    /// Provided folder path belongs to on-demand sync root with characteristics different from expected ones
    /// </summary>
    Invalid,

    /// <summary>
    /// Failed to obtain and verify on-demand sync root for the provided folder path
    /// </summary>
    VerificationFailed,
}
