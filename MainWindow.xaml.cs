using System.Windows;
using System.Windows.Threading;
using QuietBar.Models;
using QuietBar.Services;

namespace QuietBar;

public partial class MainWindow : Window
{
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
        }, DispatcherPriority.ApplicationIdle);
    }

    private void RefreshHardwareSnapshot()
    {
        var snapshot = _hardwareMonitorService.GetSnapshot();

        CollapsedCpuText.Text = $"CPU {FormatPercent(snapshot.CpuUsage)}";
        CollapsedGpuText.Text = $"GPU {FormatPercent(snapshot.GpuUsage)}";
        RamText.Text = $"RAM  {FormatPercent(snapshot.MemoryUsage)}";
        VramText.Text = $"VRAM {FormatPercent(snapshot.GpuMemoryUsage)}";
        TemperatureText.Text = $"TEMP {FormatTemperature(snapshot.GpuTemperature)}";
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

        Dispatcher.BeginInvoke(() =>
        {
            _taskbarPositionService.PlaceWindow(this, _settings.Position);
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
}
