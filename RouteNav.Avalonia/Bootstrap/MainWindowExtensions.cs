using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace NSE.RouteNav.Bootstrap;

public static class MainWindowExtensions
{
    public static void SetMainWindow(this IApplicationLifetime? lifetime, Window mainWindow, bool initMainRoute = true)
    {
        if (lifetime == null)
            throw new ArgumentNullException(nameof(lifetime), "IApplicationLifetime must not be null.");

        if (lifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime)
            desktopLifetime.SetMainWindow(mainWindow, null, initMainRoute);
        else if (lifetime is ISingleViewApplicationLifetime singleViewLifetime)
            singleViewLifetime.SetMainWindow(mainWindow, initMainRoute);
        else
            throw new NotSupportedException($"IApplicationLifetime of type '{lifetime.GetType()}' not supported.");
    }

    public static void SetMainWindow(this IClassicDesktopStyleApplicationLifetime desktopLifetime, Window mainWindow, Action<Avalonia.Controls.Window>? windowCustomization = null, bool initMainRoute = true)
    {
        desktopLifetime.MainWindow = mainWindow.ToPlatformWindow(desktopLifetime, windowCustomization);

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
        if (singleViewLifetime.MainView is Window previousWindow)
            previousWindow.OnClosed();

        singleViewLifetime.MainView = mainWindow.ToPlatformView(singleViewLifetime);
        mainWindow.OnOpened();

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

    public static Window GetMainWindow(this Application application)
    {
        return application.ApplicationLifetime!.GetMainWindow();
    }

    public static Window GetMainWindow(this IApplicationLifetime lifetime)
    {
        if (lifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow?.Tag as Window
                   ?? throw new ApplicationException("Application does not specify a MainWindow.");
        }

        if (lifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            return singleViewLifetime.MainView?.Tag as Window
                   ?? throw new ApplicationException("Application does not specify a MainWindow.");
        }

        throw new NotSupportedException($"IApplicationLifetime of type '{lifetime.GetType()}' not supported.");
    }
}
