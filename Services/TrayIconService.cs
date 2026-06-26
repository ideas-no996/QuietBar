using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;

namespace QuietBar.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly SettingsService _settingsService;
    private readonly StartupService _startupService;
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ToolStripMenuItem _visibilityItem;
    private readonly Forms.ToolStripMenuItem _startupItem;

    public TrayIconService(MainWindow mainWindow, SettingsService settingsService, StartupService startupService)
    {
        _mainWindow = mainWindow;
        _settingsService = settingsService;
        _startupService = startupService;

        _visibilityItem = new Forms.ToolStripMenuItem("隐藏 QuietBar", null, ToggleVisibility);
        var reloadItem = new Forms.ToolStripMenuItem("重新加载配置", null, (_, _) => _mainWindow.ReloadSettings());
        _startupItem = new Forms.ToolStripMenuItem("开机自启动", null, ToggleStartup);
        var exitItem = new Forms.ToolStripMenuItem("退出", null, ExitApplication);

        var menu = new Forms.ContextMenuStrip();
        menu.Opening += (_, _) => RefreshMenuState();
        menu.Items.Add(_visibilityItem);
        menu.Items.Add(reloadItem);
        menu.Items.Add(_startupItem);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(exitItem);

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "QuietBar",
            ContextMenuStrip = menu,
            Visible = true
        };

        _notifyIcon.DoubleClick += ToggleVisibility;
    }

    private void ToggleVisibility(object? sender, EventArgs e)
    {
        if (_mainWindow.IsVisible)
        {
            _mainWindow.Hide();
        }
        else
        {
            _mainWindow.Show();
            _mainWindow.Activate();
        }
    }

    private void ToggleStartup(object? sender, EventArgs e)
    {
        _startupService.SetEnabled(!_startupService.IsEnabled());
        RefreshMenuState();
    }

    private void RefreshMenuState()
    {
        _visibilityItem.Text = _mainWindow.IsVisible ? "隐藏 QuietBar" : "显示 QuietBar";
        _startupItem.Checked = _startupService.IsEnabled();
    }

    private void ExitApplication(object? sender, EventArgs e)
    {
        _notifyIcon.Visible = false;
        System.Windows.Application.Current.Shutdown();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
