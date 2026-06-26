using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;
using QuietBar.Models;

namespace QuietBar.Services;

public sealed class HardwareMonitorService : IDisposable
{
    private readonly Computer _computer;

    public HardwareMonitorService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = false
        };

        try
        {
            _computer.Open();
        }
        catch
        {
            // Hardware access can fail on locked-down machines. The UI will show N/A.
        }
    }

    public HardwareSnapshot GetSnapshot()
    {
        try
        {
            foreach (var hardware in _computer.Hardware)
            {
                UpdateHardwareTree(hardware);
            }

            return new HardwareSnapshot
            {
                CpuUsage = FindCpuUsage(),
                MemoryUsage = GetMemoryUsage(),
                GpuUsage = FindGpuUsage(),
                GpuMemoryUsage = FindGpuMemoryUsage(),
                GpuTemperature = FindGpuTemperature()
            };
        }
        catch
        {
            return new HardwareSnapshot
            {
                MemoryUsage = GetMemoryUsage()
            };
        }
    }

    private static void UpdateHardwareTree(IHardware hardware)
    {
        hardware.Update();

        foreach (var subHardware in hardware.SubHardware)
        {
            UpdateHardwareTree(subHardware);
        }
    }

    private float? FindCpuUsage()
    {
        return Sensors()
            .Where(sensor => sensor.SensorType == SensorType.Load &&
                             sensor.Hardware.HardwareType == HardwareType.Cpu)
            .OrderByDescending(sensor => sensor.Name.Contains("Total", StringComparison.OrdinalIgnoreCase))
            .Select(sensor => sensor.Value)
            .FirstOrDefault(value => value.HasValue);
    }

    private float? FindGpuUsage()
    {
        return GpuSensors()
            .Where(sensor => sensor.SensorType == SensorType.Load &&
                             sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
            .Select(sensor => sensor.Value)
            .FirstOrDefault(value => value.HasValue)
            ?? GpuSensors()
                .Where(sensor => sensor.SensorType == SensorType.Load &&
                                 sensor.Name.Contains("GPU", StringComparison.OrdinalIgnoreCase))
                .Select(sensor => sensor.Value)
                .FirstOrDefault(value => value.HasValue);
    }

    private float? FindGpuMemoryUsage()
    {
        return GpuSensors()
            .Where(sensor => sensor.SensorType == SensorType.Load &&
                             (sensor.Name.Contains("Memory", StringComparison.OrdinalIgnoreCase) ||
                              sensor.Name.Contains("VRAM", StringComparison.OrdinalIgnoreCase)))
            .Select(sensor => sensor.Value)
            .FirstOrDefault(value => value.HasValue);
    }

    private float? FindGpuTemperature()
    {
        return GpuSensors()
            .Where(sensor => sensor.SensorType == SensorType.Temperature)
            .OrderByDescending(sensor => sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
            .Select(sensor => sensor.Value)
            .FirstOrDefault(value => value.HasValue);
    }

    private IEnumerable<ISensor> GpuSensors()
    {
        return Sensors().Where(sensor => sensor.Hardware.HardwareType is HardwareType.GpuNvidia
            or HardwareType.GpuAmd
            or HardwareType.GpuIntel);
    }

    private IEnumerable<ISensor> Sensors()
    {
        foreach (var hardware in _computer.Hardware)
        {
            foreach (var sensor in EnumerateSensors(hardware))
            {
                yield return sensor;
            }
        }
    }

    private static IEnumerable<ISensor> EnumerateSensors(IHardware hardware)
    {
        foreach (var sensor in hardware.Sensors)
        {
            yield return sensor;
        }

        foreach (var subHardware in hardware.SubHardware)
        {
            foreach (var sensor in EnumerateSensors(subHardware))
            {
                yield return sensor;
            }
        }
    }

    private static float? GetMemoryUsage()
    {
        var status = new MemoryStatusEx();
        if (!GlobalMemoryStatusEx(status))
        {
            return null;
        }

        return status.MemoryLoad;
    }

    public void Dispose()
    {
        try
        {
            _computer.Close();
        }
        catch
        {
            // Ignore shutdown-time hardware monitor failures.
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx lpBuffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;

        public MemoryStatusEx()
        {
            Length = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
        }
    }
}
