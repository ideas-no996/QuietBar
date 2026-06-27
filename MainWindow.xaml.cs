using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using QuietBar.Models;
using QuietBar.Services;

namespace QuietBar;

public partial class MainWindow : Window
{
    private const double CollapsedWidth = 148;
    private const double ExpandedWidth = 392;
    private readonly SettingsService _settingsService;
    private readonly HardwareMonitorService _hardwareMonitorService = new();
    private readonly TaskbarPositionService _taskbarPositionService = new();
    private readonly DispatcherTimer _refreshTimer = new();
    private readonly DispatcherTimer _collapseTimer = new();
    private AppSettings _settings;
    private bool _isExpanded;

    public MainWindow(SettingsService settingsService, AppSettings settings)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _settings = settings;

        _refreshTimer.Tick += (_, _) => RefreshHardwareSnapshot();
        _collapseTimer.Interval = TimeSpan.FromSeconds(2);
        _collapseTimer.Tick += (_, _) =>
        {
            _collapseTimer.Stop();
            SetExpanded(false);
        };

        Loaded += (_, _) =>
        {
            ApplySettings(_settings);
            RefreshHardwareSnapshot();
            _refreshTimer.Start();
        };

        Closed += (_, _) =>
        {
            _refreshTimer.Stop();
            _collapseTimer.Stop();
            _hardwareMonitorService.Dispose();
        };
    }

    public void ReloadSettings()
    {
        ApplySettings(_settingsService.Load());
    }

    private void ApplySettings(AppSettings settings)
    {
        _settings = settings;
        Topmost = _settings.AlwaysOnTop;
        Opacity = _isExpanded ? _settings.ExpandedOpacity : _settings.CollapsedOpacity;
        Width = _isExpanded ? ExpandedWidth : CollapsedWidth;

        var interval = Math.Max(250, _settings.RefreshIntervalMs);
        _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);

        CollapsedCpuText.FontSize = _settings.FontSize;
        CollapsedGpuText.FontSize = _settings.FontSize;
        RamText.FontSize = _settings.FontSize;
        VramText.FontSize = _settings.FontSize;
        TemperatureText.FontSize = _settings.FontSize;

        Dispatcher.BeginInvoke(() =>
        {
            _taskbarPositionService.PlaceWindow(this, _settings.Position);
            EnsureTopmostWithoutActivation();
        }, DispatcherPriority.ApplicationIdle);
    }

    private void RefreshHardwareSnapshot()
    {
        var snapshot = _hardwareMonitorService.GetSnapshot();

        CollapsedCpuText.Text = $"CPU {FormatPercent(snapshot.CpuUsage)}";
        CollapsedGpuText.Text = $"GPU {FormatPercent(snapshot.GpuUsage)}";
        RamText.Text = FormatPercent(snapshot.MemoryUsage);
        VramText.Text = FormatPercent(snapshot.GpuMemoryUsage);
        TemperatureText.Text = FormatTemperature(snapshot.GpuTemperature);
    }

    private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _collapseTimer.Stop();
        SetExpanded(true);
    }

    private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _collapseTimer.Stop();
        _collapseTimer.Start();
    }

    private void SetExpanded(bool expanded)
    {
        _isExpanded = expanded;
        ExpandedPanel.Visibility = expanded ? Visibility.Visible : Visibility.Collapsed;
        Opacity = expanded ? _settings.ExpandedOpacity : _settings.CollapsedOpacity;
        Width = expanded ? ExpandedWidth : CollapsedWidth;

        Dispatcher.BeginInvoke(() =>
        {
            _taskbarPositionService.PlaceWindow(this, _settings.Position);
            EnsureTopmostWithoutActivation();
        }, DispatcherPriority.ApplicationIdle);
    }

    private static string FormatPercent(float? value)
    {
        return value.HasValue ? $"{Math.Round(value.Value):0}%" : "N/A";
    }

    private static string FormatTemperature(float? value)
    {
        return value.HasValue ? $"{Math.Round(value.Value):0}C" : "N/A";
    }

    private void EnsureTopmostWithoutActivation()
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        SetWindowPos(handle, HwndTopmost, 0, 0, 0, 0, SetWindowPosFlags);
    }

    private static readonly IntPtr HwndTopmost = new(-1);
    private const uint SetWindowPosFlags = 0x0001 | 0x0002 | 0x0010 | 0x0040;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);
}
