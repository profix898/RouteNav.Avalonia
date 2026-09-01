using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Internal;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Platform;

/// <summary>
/// Default <see cref="IWindowManager" /> implementation for Avalonia desktop and single-view platforms.
/// </summary>
public class AvaloniaWindowManager : IWindowManager
{
    private static IApplicationLifetime? ApplicationLifetime => Application.Current!.ApplicationLifetime;

    /// <inheritdoc />
    public virtual bool SupportsMultiWindow => ApplicationLifetime is IClassicDesktopStyleApplicationLifetime;

    /// <inheritdoc />
    public event IWindowManager.WindowCustomizationHandler WindowCustomizationEvent;

    /// <inheritdoc />
    public virtual bool OpenWindow(Window window, Window? parentWindow = null)
    {
        // Variant A: Desktop (multi-window) platform
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime && !Navigation.Windows.ForceSingleWindow)
        {
            var platformWindow = CreatePlatformWindow(window, desktopLifetime);

            // Show navigation windows unowned: Windows keeps owned windows permanently above their
            // owner, so an owned secondary window could never be raised below/above the main window
            // during cross-window navigation. Dialog windows keep their modal owner (ShowDialog).
            platformWindow.Show();

            return true;
        }

        // Variant B: Mobile/Browser (single window/view) platform + Fallback -> no window support
        return false;
    }

    /// <inheritdoc />
    public virtual bool OpenDialog(Dialog dialog, out Task<object?> dialogTask, Window? parentWindow = null)
    {
        // Variant A: Desktop (multi-window) platform -> open dialog window
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime && !Navigation.Windows.ForceOverlayDialogs)
        {
            var ownerRouteWindow = parentWindow ?? Application.Current!.GetMainWindow();
            if (ownerRouteWindow.IsClosed)
            {
                // The owner window was closed and has not been re-opened -> fall back to overlay display
                dialogTask = Task.FromResult<object?>(null);
                return false;
            }

            var ownerWindow = ownerRouteWindow.PlatformControl as AvaloniaWindow ?? desktopLifetime.MainWindow;
            if (ownerWindow == null)
                throw new NavigationException("No main window/view available. Application not fully initialized yet.");

            dialog.SetSize(ownerRouteWindow);

            var activeStack = Navigation.UIPlatform.GetActiveStackFromWindow(ownerRouteWindow);
            var dialogWindow =
                Navigation.UIPlatform.CreateWindow(new WindowCreationContext(WindowKind.Dialog, activeStack, ownerRouteWindow, dialog, dialog.Title, ownerRouteWindow.Icon));
            var platformWindow = CreatePlatformWindow(dialogWindow, desktopLifetime, true);

            dialog.RegisterPlatform(platformWindow);
            dialogTask = dialog.Open();

            platformWindow.ShowDialog(ownerWindow);
            return true;
        }

        // Variant B: Mobile/Browser (single window/view) platform + fallback -> open dialog in overlay
        dialogTask = Task.FromResult<object?>(null);
        return false;
    }

    /// <inheritdoc />
    public virtual AvaloniaWindow CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime desktopLifetime, bool isDialogWindow = false)
    {
        var platformWindow = new AvaloniaWindow { Title = window.Title, Icon = window.Icon, Tag = window, Content = window };

        // Mirror declarative window chrome (XAML-declared on the window shell) onto the platform
        // window. Runs before the customization event so event handlers can override everything.
        ApplyWindowChrome(window, platformWindow);

        if (isDialogWindow && window.Content is Dialog dialog)
        {
            platformWindow.CanResize = false;
            platformWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            platformWindow.VerticalContentAlignment = VerticalAlignment.Stretch;

            // Explicit axes size the window; content-hug axes (NaN) size the window to its content
            if (!Double.IsNaN(dialog.Width))
            {
                platformWindow.Width = dialog.Width;
                platformWindow.MinWidth = dialog.Width;
            }
            if (!Double.IsNaN(dialog.Height))
            {
                platformWindow.Height = dialog.Height;
                platformWindow.MinHeight = dialog.Height;
            }

            platformWindow.SizeToContent = (Double.IsNaN(dialog.Width), Double.IsNaN(dialog.Height)) switch
            {
                (true, true) => SizeToContent.WidthAndHeight,
                (true, false) => SizeToContent.Width,
                (false, true) => SizeToContent.Height,
                _ => SizeToContent.Manual
            };

            platformWindow.SetDialogStyle();
        }

        WindowCustomizationEvent?.Invoke(platformWindow, isDialogWindow);
        window.RegisterPlatform(desktopLifetime, platformWindow);

        return platformWindow;
    }

    /// <summary>
    /// Mirrors declarative window chrome from the window shell onto the platform window: size
    /// properties describe the OS window (layout sizes of the hosted shell are consumed), all
    /// other chrome properties are applied unless the shell leaves them at their defaults.
    /// </summary>
    private static void ApplyWindowChrome(Window window, AvaloniaWindow platformWindow)
    {
        platformWindow.CanResize = window.CanResize;
        platformWindow.CanMinimize = window.CanMinimize;
        platformWindow.CanMaximize = window.CanMaximize;
        platformWindow.ShowInTaskbar = window.ShowInTaskbar;
        platformWindow.ShowActivated = window.ShowActivated;
        platformWindow.Topmost = window.Topmost;
        platformWindow.WindowState = window.WindowState;
        platformWindow.WindowStartupLocation = window.WindowStartupLocation;
        platformWindow.WindowDecorations = window.WindowDecorations;
        platformWindow.ExtendClientAreaToDecorationsHint = window.ExtendClientAreaToDecorationsHint;

        if (window.IsSet(Window.TransparencyLevelHintProperty))
            platformWindow.TransparencyLevelHint = window.TransparencyLevelHint;
        if (window.IsSet(Window.TransparencyBackgroundFallbackProperty))
            platformWindow.TransparencyBackgroundFallback = window.TransparencyBackgroundFallback;

        // Layout sizes declared on the shell describe the OS window, not the hosted shell:
        // width/height are transferred and cleared (the shell then fills the client area),
        // min/max sizes are mirrored and kept.
        if (window.IsSet(Layoutable.WidthProperty))
        {
            platformWindow.Width = window.Width;
            window.ClearValue(Layoutable.WidthProperty);
        }
        if (window.IsSet(Layoutable.HeightProperty))
        {
            platformWindow.Height = window.Height;
            window.ClearValue(Layoutable.HeightProperty);
        }
        if (window.IsSet(Layoutable.MinWidthProperty))
            platformWindow.MinWidth = window.MinWidth;
        if (window.IsSet(Layoutable.MinHeightProperty))
            platformWindow.MinHeight = window.MinHeight;
        if (window.IsSet(Layoutable.MaxWidthProperty))
            platformWindow.MaxWidth = window.MaxWidth;
        if (window.IsSet(Layoutable.MaxHeightProperty))
            platformWindow.MaxHeight = window.MaxHeight;
    }

    /// <inheritdoc />
    public virtual ContentControl CreatePlatformView(Window window, IApplicationLifetime appLifetime)
    {
        var platformControl = new UserControl
        {
            // Host the RouteNav window: it joins the visual/logical tree of the host view, so
            // window-level XAML resolves natively against the application resource chain.
            Tag = window, Content = window
        };

        window.RegisterPlatform(appLifetime, platformControl);

        return platformControl;
    }
}
