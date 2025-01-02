namespace ProtonDrive.App.Instrumentation.Telemetry.MappingSetup;

public enum MappingSyncType
{
    /// <summary>
    /// From remote to local sync. It is used for read-only "shared with me" items.
    /// </summary>
    OneWayToLocal,

    /// <summary>
    /// Default two-way synchronization.
    /// </summary>
    TwoWay,
}
