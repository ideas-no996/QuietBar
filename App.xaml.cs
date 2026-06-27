using QuietBar.Services;
using System.Windows;
using System.Windows.Threading;

namespace QuietBar;

public partial class App : System.Windows.Application
{
    private MainWindow? _mainWindow;
    private TrayIconService? _trayIconService;
    private bool _errorShown;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            var settingsService = new SettingsService();
            var settings = settingsService.Load();
            var startupService = new StartupService();

            _mainWindow = new MainWindow(settingsService, settings);
            _trayIconService = new TrayIconService(_mainWindow, settingsService, startupService);

            _mainWindow.Show();
        }
        catch (Exception ex)
        {
            ShowSingleError("QuietBar could not start.", ex);
            Shutdown();
        }
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _trayIconService?.Dispose();
        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        ShowSingleError("QuietBar hit a recoverable problem and will keep running if possible.", e.Exception);
    }

    private void ShowSingleError(string message, Exception exception)
    {
        if (_errorShown)
        {
            return;
        }

        _errorShown = true;
        System.Windows.MessageBox.Show(
            $"{message}\n\n{exception.Message}",
            "QuietBar",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}
