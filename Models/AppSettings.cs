namespace QuietBar.Models;

public sealed class AppSettings
{
    public int RefreshIntervalMs { get; set; } = 1000;
    public double CollapsedOpacity { get; set; } = 0.55;
    public double ExpandedOpacity { get; set; } = 1.0;
    public int CollapseDelayMs { get; set; } = 800;
    public double FontSize { get; set; } = 12;
    public string Position { get; set; } = "bottom-left";
    public bool AlwaysOnTop { get; set; } = true;
}
