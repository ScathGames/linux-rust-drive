using System.Text.Json.Serialization;

namespace ProtonDrive.Client.Contracts;

public sealed class Block
{
    public int Index { get; init; }

    [JsonPropertyName("URL")]
    public string? Url { get; init; }

    [JsonPropertyName("EncSignature")]
    public string? EncryptedSignature { get; init; }

    [JsonPropertyName("SignatureEmail")]
    public string? SignatureEmailAddress { get; init; }
}
