using System.Text.Json.Serialization;

namespace ProtonDrive.Client.Contracts;

public sealed record DefaultPlanResponse : ApiResponse
{
    private readonly Plan? _plan;

    [JsonPropertyName("Plans")]
    public Plan Plan
    {
        get => _plan ?? throw new ApiException("Plan is not set");
        init => _plan = value;
    }
}
