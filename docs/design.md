# QuietBar Design Notes

## Low Distraction

QuietBar should feel like quiet ambient instrumentation, not a dashboard demanding attention. The default state is semi-transparent and compact, showing only CPU and GPU usage.

## Semi-Hidden by Default

The window starts in collapsed mode with low opacity. Hovering expands it immediately, and leaving the window starts a short delay before it collapses again. This keeps extra details nearby without making them persistent visual noise.

## Readability First

The background uses a dark translucent rounded rectangle with a light border and subtle shadow so text remains readable over both light and dark taskbars. Text uses high contrast and a small shadow for mixed desktop backgrounds.

## Read-Only Monitoring

QuietBar only reads status information. It does not clean memory, optimize performance, tune hardware, kill processes, change drivers, or modify system behavior.

## Graceful Degradation

Hardware sensor support varies by device. Missing GPU, VRAM, or temperature data should be displayed as `N/A`; the app should continue running.

## Small First Version

The first version intentionally avoids charts, history, alerts, and system tools. The core experience is a stable small status surface with a tray menu for basic control.
