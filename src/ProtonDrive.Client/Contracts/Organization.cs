using System.Text.Json.Serialization;

namespace ProtonDrive.Client.Contracts;

public sealed class Organization
{
    public string? Name { get; set; }

    public string? DisplayName { get; set; }

    [JsonPropertyName("PlanName")]
    public string? PlanCode { get; set; }
}
