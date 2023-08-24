using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RouteNav.Avalonia.Dialogs;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Platform;

public interface IWindowManager
{
    bool SupportsMultiWindow { get; }

    bool ForceSingleWindow { get; init; }

    bool ForceOverlayDialogs { get; init; }

    bool OpenWindow(Window window, Window? parentWindow = null);

    bool OpenDialog(Dialog dialog, out Task<object?> dialogTask, Window? parentWindow = null);

    AvaloniaWindow CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime desktopLifetime, Action<AvaloniaWindow>? windowCustomization = null);

    ContentControl CreatePlatformView(Window window, ISingleViewApplicationLifetime singleViewLifetime);
}
