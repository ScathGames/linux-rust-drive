namespace ProtonDrive.Client.Authentication.Contracts;

internal sealed record TwoFactor
{
    public int Enabled { get; init; }
}
