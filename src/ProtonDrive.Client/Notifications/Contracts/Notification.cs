using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProtonDrive.Client.Notifications.Contracts;

public sealed class Notification
{
    [JsonPropertyName("NotificationID")]
    public required string Id { get; init; }

    public IReadOnlyCollection<string> UserSubscriptionPlanCodes { get; init; } = new List<string>();

    [JsonPropertyName("StartTime")]
    public required DateTimeOffset StartTime { get; init; }

    [JsonPropertyName("EndTime")]
    public required DateTimeOffset EndTime { get; init; }

    public NotificationType Type { get; init; }

    public string? HeaderText { get; set; }
    public string? ContentText { get; set; }
    public string? ButtonText { get; set; }
    public string? LogoImageUrl { get; init; }

    public Offer? Offer { get; init; }
}
