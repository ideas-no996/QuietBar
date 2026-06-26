using QuietBar.Services;

namespace QuietBar;

public partial class App : System.Windows.Application
{
    private MainWindow? _mainWindow;
    private TrayIconService? _trayIconService;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        var settingsService = new SettingsService();
        var settings = settingsService.Load();
        var startupService = new StartupService();

        _mainWindow = new MainWindow(settingsService, settings);
        _trayIconService = new TrayIconService(_mainWindow, settingsService, startupService);

        _mainWindow.Show();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _trayIconService?.Dispose();
        base.OnExit(e);
    }
}
