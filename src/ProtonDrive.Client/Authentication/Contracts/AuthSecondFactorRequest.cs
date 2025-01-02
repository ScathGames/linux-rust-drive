namespace ProtonDrive.Client.Authentication.Contracts;

internal sealed record AuthSecondFactorRequest
{
    public string TwoFactorCode { get; init; } = string.Empty;
}
