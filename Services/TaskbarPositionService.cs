using System.Windows;

namespace QuietBar.Services;

public sealed class TaskbarPositionService
{
    private const double LeftInset = 24;
    private const double FallbackMargin = 8;
    private const double MinimumTaskbarHeight = 32;
    private const double MaximumTaskbarHeight = 120;

    public void PlaceWindow(Window window, string position)
    {
        if (!string.Equals(position, "bottom-left", StringComparison.OrdinalIgnoreCase))
        {
            position = "bottom-left";
        }

        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var workArea = SystemParameters.WorkArea;
        var windowHeight = window.ActualHeight > 0 ? window.ActualHeight : window.Height;
        var windowWidth = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
        var bottomTaskbarHeight = screenHeight - workArea.Bottom;

        window.Left = Math.Clamp(LeftInset, 0, Math.Max(0, screenWidth - windowWidth - FallbackMargin));

        if (bottomTaskbarHeight is >= MinimumTaskbarHeight and <= MaximumTaskbarHeight)
        {
            var centeredInTaskbar = workArea.Bottom + ((bottomTaskbarHeight - windowHeight) / 2);
            window.Top = Math.Clamp(centeredInTaskbar, workArea.Bottom, screenHeight - windowHeight);
            return;
        }

        window.Top = Math.Clamp(workArea.Bottom - windowHeight - FallbackMargin, 0, screenHeight - windowHeight);
    }
}
