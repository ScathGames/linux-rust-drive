using System.Text.Json.Serialization;
using ProtonDrive.App.Mapping;

namespace ProtonDrive.App.Settings;

public sealed class StorageOptimizationState
{
    public bool IsEnabled { get; set; }

    public StorageOptimizationStatus Status { get; set; }

    [JsonIgnore]
    public StorageOptimizationErrorCode ErrorCode { get; set; }

    [JsonIgnore]
    public string? ConflictingProviderName { get; set; }
}
