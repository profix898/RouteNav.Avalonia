using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Platform;

public interface IWindowManager
{
    bool OpenWindow(Window window, Window? parentWindow = null);

    Task<object> OpenDialog(Dialogs.Dialog dialog, Window? parentWindow = null);

    AvaloniaWindow CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime desktopLifetime, Action<AvaloniaWindow>? windowCustomization = null);

    ContentControl CreatePlatformView(Window window, ISingleViewApplicationLifetime singleViewLifetime);
}
