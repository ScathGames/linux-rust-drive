using System.Text.Json.Serialization;

namespace ProtonDrive.Client.Settings.Contracts;

public sealed record SettingsResponse : ApiResponse
{
    private readonly GeneralSettings? _settings;

    [JsonPropertyName("UserSettings")]
    public GeneralSettings Settings
    {
        get => _settings ?? throw new ApiException("Settings is not set");
        init => _settings = value;
    }
}
