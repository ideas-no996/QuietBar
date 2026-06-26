namespace QuietBar.Models;

public sealed class HardwareSnapshot
{
    public float? CpuUsage { get; init; }
    public float? MemoryUsage { get; init; }
    public float? GpuUsage { get; init; }
    public float? GpuMemoryUsage { get; init; }
    public float? GpuTemperature { get; init; }
}
