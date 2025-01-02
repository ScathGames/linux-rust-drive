namespace ProtonDrive.Client.Authentication.Sessions;

internal sealed record SessionForkingResponse : ApiResponse
{
    public string Selector { get; init; } = string.Empty;
}
