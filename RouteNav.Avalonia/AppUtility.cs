using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia;

/// <summary>
/// Helper extensions for wiring RouteNav's <see cref="Window" /> abstraction into the various Avalonia
/// application lifetimes (desktop, single-view and activity), and for retrieving the main window/top level.
/// </summary>
public static class AppUtility
{
    // Tracks the RouteNav main-window abstraction independently of the platform lifetime, so that
    // GetMainWindow()/GetTopLevel() keep working regardless of whether the platform sets the view
    // directly (ISingleViewApplicationLifetime.MainView) or lazily via a factory
    // (IActivityApplicationLifetime.MainViewFactory, used by Avalonia 12 on Android).
    private static Window? mainWindow;

    #region SetMainWindow

    /// <summary>Sets the RouteNav main window on the given application lifetime, dispatching to the matching lifetime type.</summary>
    /// <param name="lifetime">The application lifetime (desktop, activity or single-view).</param>
    /// <param name="newMainWindow">The main window abstraction to host.</param>
    /// <param name="initMainRoute">When <c>true</c>, immediately enters the main navigation stack.</param>
    public static void SetMainWindow(this IApplicationLifetime? lifetime, Window newMainWindow, bool initMainRoute = true)
    {
        if (lifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime)
            desktopLifetime.SetMainWindow(newMainWindow, initMainRoute);

        // Note: Avalonia 12's Android lifetime implements both IActivityApplicationLifetime and
        // ISingleViewApplicationLifetime. Prefer the factory-based activity path (avoids the
        // "MainView is not fully supported on Android" warning and supports multiple activities).
        else if (lifetime is IActivityApplicationLifetime activityLifetime)
            activityLifetime.SetMainWindow(newMainWindow, initMainRoute);
        else if (lifetime is ISingleViewApplicationLifetime singleViewLifetime)
            singleViewLifetime.SetMainWindow(newMainWindow, initMainRoute);
        else if (!Design.IsDesignMode)
            throw new NotSupportedException($"IApplicationLifetime of type '{lifetime?.GetType()}' not supported.");
    }

    /// <summary>Sets the RouteNav main window on a desktop lifetime.</summary>
    public static void SetMainWindow(this IClassicDesktopStyleApplicationLifetime desktopLifetime, Window newMainWindow, bool initMainRoute = true)
    {
        (desktopLifetime.MainWindow?.Tag as Window ?? mainWindow)?.OnClosed();
        mainWindow = newMainWindow;

        var windowManager = Navigation.UIPlatform.WindowManager;
        desktopLifetime.MainWindow = windowManager.CreatePlatformWindow(newMainWindow, desktopLifetime);

        if (initMainRoute)
            EnterMainStack();
    }

    /// <summary>Sets the RouteNav main window on an activity lifetime (Avalonia 12 Android) via its main-view factory.</summary>
    public static void SetMainWindow(this IActivityApplicationLifetime activityLifetime, Window newMainWindow, bool initMainRoute = true)
    {
        mainWindow?.OnClosed();
        mainWindow = newMainWindow;

        var windowManager = Navigation.UIPlatform.WindowManager;
        activityLifetime.MainViewFactory = () => windowManager.CreatePlatformView(newMainWindow, activityLifetime);

        if (initMainRoute)
            EnterMainStack();
    }

    /// <summary>Sets the RouteNav main window on a single-view lifetime (mobile/browser).</summary>
    public static void SetMainWindow(this ISingleViewApplicationLifetime singleViewLifetime, Window newMainWindow, bool initMainRoute = true)
    {
        (singleViewLifetime.MainView?.Tag as Window ?? mainWindow)?.OnClosed();
        mainWindow = newMainWindow;

        var windowManager = Navigation.UIPlatform.WindowManager;
        singleViewLifetime.MainView = windowManager.CreatePlatformView(newMainWindow, singleViewLifetime);

        if (initMainRoute)
            EnterMainStack();
    }

    private static void EnterMainStack()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
#pragma warning disable CS4014
            Navigation.EnterStack();
#pragma warning restore CS4014
        });
    }

    #endregion

    #region GetMainWindow

    /// <summary>Gets the RouteNav main window for the given application.</summary>
    public static Window GetMainWindow(this Application application)
    {
        return application.ApplicationLifetime!.GetMainWindow();
    }

    /// <summary>Gets the RouteNav main window tracked for the given application lifetime.</summary>
    public static Window GetMainWindow(this IApplicationLifetime appLifetime)
    {
        if (mainWindow != null)
            return mainWindow;

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

    /// <summary>Gets the active <see cref="TopLevel" /> (the given window, the active desktop window, or the single-view host).</summary>
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

        // Single-view / activity platforms: resolve the TopLevel from the hosted platform control.
        var platformControl = mainWindow?.PlatformControl
                              ?? (appLifetime as ISingleViewApplicationLifetime)?.MainView;
        if (platformControl != null)
        {
            return TopLevel.GetTopLevel(platformControl)
                   ?? throw new ApplicationException("Application does not specify a MainView.");
        }

        throw new NotSupportedException($"IApplicationLifetime of type '{appLifetime?.GetType()}' not supported.");
    }

    #endregion
}
