namespace ProtonDrive.App.SystemIntegration;

public sealed record OnDemandSyncRootVerificationResult(OnDemandSyncRootVerificationVerdict Verdict, string? ConflictingProviderName = null);
