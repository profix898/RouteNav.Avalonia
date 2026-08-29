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
    /// <param name="windowFactory">Factory that creates a fresh main window abstraction whenever the platform needs one.</param>
    /// <param name="initMainRoute">When <c>true</c>, immediately enters the main navigation stack.</param>
    public static void SetMainWindow(this IApplicationLifetime? lifetime, WindowFactory windowFactory, bool initMainRoute = true)
    {
        if (lifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime)
            desktopLifetime.SetMainWindow(windowFactory, initMainRoute);

        // Note: Avalonia 12's Android lifetime implements both IActivityApplicationLifetime and
        // ISingleViewApplicationLifetime. Prefer the factory-based activity path (avoids the
        // "MainView is not fully supported on Android" warning and supports multiple activities).
        else if (lifetime is IActivityApplicationLifetime activityLifetime)
            activityLifetime.SetMainWindow(windowFactory, initMainRoute);
        else if (lifetime is ISingleViewApplicationLifetime singleViewLifetime)
            singleViewLifetime.SetMainWindow(windowFactory, initMainRoute);
        else if (!Design.IsDesignMode)
            throw new NotSupportedException($"IApplicationLifetime of type '{lifetime?.GetType()}' not supported.");
    }

    /// <summary>Sets the RouteNav main window on a desktop lifetime.</summary>
    public static void SetMainWindow(this IClassicDesktopStyleApplicationLifetime desktopLifetime, WindowFactory windowFactory, bool initMainRoute = true)
    {
        Navigation.UIPlatform.DefaultWindowFactory = windowFactory ?? throw new ArgumentNullException(nameof(windowFactory));

        var previousWindow = desktopLifetime.MainWindow?.Tag as Window ?? mainWindow;
        var newMainWindow = CreateMainWindow();
        previousWindow?.OnClosed();
        mainWindow = newMainWindow;

        var windowManager = Navigation.UIPlatform.WindowManager;
        desktopLifetime.MainWindow = windowManager.CreatePlatformWindow(newMainWindow, desktopLifetime);

        var transferredStack = previousWindow != null && Navigation.UIPlatform.ReplaceActiveWindow(previousWindow, newMainWindow);
        if (initMainRoute && !transferredStack)
            EnterMainStack();
    }

    /// <summary>Sets the RouteNav main window on an activity lifetime (Avalonia 12 Android) via its main-view factory.</summary>
    public static void SetMainWindow(this IActivityApplicationLifetime activityLifetime, WindowFactory windowFactory, bool initMainRoute = true)
    {
        Navigation.UIPlatform.DefaultWindowFactory = windowFactory ?? throw new ArgumentNullException(nameof(windowFactory));

        var windowManager = Navigation.UIPlatform.WindowManager;
        activityLifetime.MainViewFactory = () =>
        {
            var previousWindow = mainWindow;
            var newMainWindow = CreateMainWindow();
            previousWindow?.OnClosed();
            mainWindow = newMainWindow;

            var platformView = windowManager.CreatePlatformView(newMainWindow, activityLifetime);
            var transferredStack = previousWindow != null && Navigation.UIPlatform.ReplaceActiveWindow(previousWindow, newMainWindow);
            if (initMainRoute && !transferredStack)
                EnterMainStack();

            return platformView;
        };
    }

    /// <summary>Sets the RouteNav main window on a single-view lifetime (mobile/browser).</summary>
    public static void SetMainWindow(this ISingleViewApplicationLifetime singleViewLifetime, WindowFactory windowFactory, bool initMainRoute = true)
    {
        Navigation.UIPlatform.DefaultWindowFactory = windowFactory ?? throw new ArgumentNullException(nameof(windowFactory));

        var previousWindow = singleViewLifetime.MainView?.Tag as Window ?? mainWindow;
        var newMainWindow = CreateMainWindow();
        previousWindow?.OnClosed();
        mainWindow = newMainWindow;

        var windowManager = Navigation.UIPlatform.WindowManager;
        singleViewLifetime.MainView = windowManager.CreatePlatformView(newMainWindow, singleViewLifetime);

        var transferredStack = previousWindow != null && Navigation.UIPlatform.ReplaceActiveWindow(previousWindow, newMainWindow);
        if (initMainRoute && !transferredStack)
            EnterMainStack();
    }

    private static Window CreateMainWindow()
    {
        return Navigation.UIPlatform.CreateWindow(new WindowCreationContext(WindowKind.Main, Navigation.UIPlatform.GetMainStack()));
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
