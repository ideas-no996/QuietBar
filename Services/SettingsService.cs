using System.IO;
using System.Text.Json;
using QuietBar.Models;

namespace QuietBar.Services;

public sealed class SettingsService
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public SettingsService()
    {
        _settingsPath = Path.Combine(AppContext.BaseDirectory, "config.json");
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                var defaults = new AppSettings();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            return Normalize(settings);
        }
        catch
        {
            return new AppSettings();
        }
    }

    private void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    private static AppSettings Normalize(AppSettings settings)
    {
        settings.RefreshIntervalMs = Math.Max(250, settings.RefreshIntervalMs);
        settings.CollapsedOpacity = Math.Clamp(settings.CollapsedOpacity, 0.1, 1.0);
        settings.ExpandedOpacity = Math.Clamp(settings.ExpandedOpacity, 0.1, 1.0);
        settings.FontSize = Math.Clamp(settings.FontSize, 9, 24);

        if (!string.Equals(settings.Position, "bottom-left", StringComparison.OrdinalIgnoreCase))
        {
            settings.Position = "bottom-left";
        }

        return settings;
    }
}
