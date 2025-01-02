using System.Text.Json.Serialization;

namespace ProtonDrive.Client.Notifications.Contracts;

public sealed class Offer
{
    [JsonPropertyName("URL")]
    public string ClickUrl { get; init; } = string.Empty;

    [JsonPropertyName("Icon")]
    public string ImageUrl { get; init; } = string.Empty;

    [JsonPropertyName("Label")]
    public string Title { get; init; } = string.Empty;

    public string? CouponCode { get; init; } = string.Empty;
}
