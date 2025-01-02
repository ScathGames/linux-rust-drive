using System.Text.Json.Serialization;

namespace ProtonDrive.Client.Contracts;

public sealed record Thumbnail(
    [property: JsonPropertyName("ThumbnailID")] string Id,
    int Type,
    int Size);
