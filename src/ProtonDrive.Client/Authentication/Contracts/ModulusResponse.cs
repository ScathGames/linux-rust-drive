using System.Text.Json.Serialization;

namespace ProtonDrive.Client.Authentication.Contracts;

internal sealed record ModulusResponse : ApiResponse
{
    public string Modulus { get; set; } = string.Empty;

    [JsonPropertyName("ModulusID")]
    public string ModulusId { get; set; } = string.Empty;
}
