using System.Text.Json.Serialization;

namespace ProtonDrive.Client.Instrumentation.Observability;

public abstract record ObservabilityMetric(
    string Name,
    int Version,
    long Timestamp,
    [property: JsonPropertyName("Data")] ObservabilityMetricProperties Properties);
