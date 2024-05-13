using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using RouteNav.Avalonia.Platform;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia;

public static class AppUtility
{
    #region SetMainWindow

    public static void SetMainWindow(this IApplicationLifetime? lifetime, Window mainWindow, bool initMainRoute = true)
    {
        if (lifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime)
            desktopLifetime.SetMainWindow(mainWindow, initMainRoute);
        else if (lifetime is ISingleViewApplicationLifetime singleViewLifetime)
            singleViewLifetime.SetMainWindow(mainWindow, initMainRoute);
        else if (!Design.IsDesignMode)
            throw new NotSupportedException($"IApplicationLifetime of type '{lifetime.GetType()}' not supported.");
    }

    public static void SetMainWindow(this IClassicDesktopStyleApplicationLifetime desktopLifetime, Window mainWindow, bool initMainRoute = true)
    {
        if (desktopLifetime.MainWindow?.Tag is Window previousWindow)
            previousWindow.OnClosed();

        IWindowManager windowManager = Navigation.UIPlatform.WindowManager;
        desktopLifetime.MainWindow = windowManager.CreatePlatformWindow(mainWindow, desktopLifetime);

        if (initMainRoute)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
#pragma warning disable CS4014
                Navigation.EnterStack();
#pragma warning restore CS4014
            });
        }
    }

    public static void SetMainWindow(this ISingleViewApplicationLifetime singleViewLifetime, Window mainWindow, bool initMainRoute = true)
    {
        if (singleViewLifetime.MainView?.Tag is Window previousWindow)
            previousWindow.OnClosed();

        IWindowManager windowManager = Navigation.UIPlatform.WindowManager;
        singleViewLifetime.MainView = windowManager.CreatePlatformView(mainWindow, singleViewLifetime);

        if (initMainRoute)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
#pragma warning disable CS4014
                Navigation.EnterStack();
#pragma warning restore CS4014
            });
        }
    }

    #endregion

    #region GetMainWindow

    public static Window GetMainWindow(this Application application)
    {
        return application.ApplicationLifetime!.GetMainWindow();
    }

    public static Window GetMainWindow(this IApplicationLifetime appLifetime)
    {
        if (appLifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow?.Tag as Window
                   ?? throw new ApplicationException("Application does not specify a MainWindow.");
        }

        if (appLifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            return singleViewLifetime.MainView?.Tag as Window
                   ?? throw new ApplicationException("Application does not specify a MainWindow.");
        }

        throw new NotSupportedException($"IApplicationLifetime of type '{appLifetime.GetType()}' not supported.");
    }

    #endregion

    #region Helpers

    public static TopLevel GetTopLevel(AvaloniaWindow? window = null)
    {
        if (window != null)
            return window;

        var appLifetime = Application.Current!.ApplicationLifetime;

        if (appLifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime && desktopLifetime.MainWindow != null)
        {
            return desktopLifetime.Windows.FirstOrDefault(wnd => wnd.IsActive)
                   ?? desktopLifetime.MainWindow
                   ?? throw new ApplicationException("Application does not specify a MainWindow.");
        }

        if (appLifetime is ISingleViewApplicationLifetime singleViewLifetime && singleViewLifetime.MainView != null)
        {
            return TopLevel.GetTopLevel(singleViewLifetime.MainView)
                   ?? throw new ApplicationException("Application does not specify a MainView.");
        }

        throw new NotSupportedException($"IApplicationLifetime of type '{appLifetime.GetType()}' not supported.");
    }

    #endregion
}
