using System.Windows;

namespace QuietBar.Services;

public sealed class TaskbarPositionService
{
    private const double Margin = 8;

    public void PlaceWindow(Window window, string position)
    {
        if (!string.Equals(position, "bottom-left", StringComparison.OrdinalIgnoreCase))
        {
            position = "bottom-left";
        }

        var workArea = SystemParameters.WorkArea;
        window.Left = workArea.Left + Margin;
        window.Top = workArea.Bottom - window.ActualHeight - Margin;
    }
}
