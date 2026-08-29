using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
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
    public bool ForceSingleWindow { get; init; }

    /// <inheritdoc />
    public bool ForceOverlayDialogs { get; init; }

    /// <inheritdoc />
    public event IWindowManager.WindowCustomizationHandler WindowCustomizationEvent;

    /// <inheritdoc />
    public virtual bool OpenWindow(Window window, Window? parentWindow = null)
    {
        // Variant A: Desktop (multi-window) platform
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime && !ForceSingleWindow)
        {
            var platformWindow = CreatePlatformWindow(window, desktopLifetime);
            var ownerWindow = parentWindow?.PlatformControl as AvaloniaWindow ?? desktopLifetime.MainWindow;
            if (ownerWindow == null)
                platformWindow.Show();
            else
                platformWindow.Show(ownerWindow);

            return true;
        }

        // Variant B: Mobile/Browser (single window/view) platform + Fallback -> no window support
        return false;
    }

    /// <inheritdoc />
    public virtual bool OpenDialog(Dialog dialog, out Task<object?> dialogTask, Window? parentWindow = null)
    {
        // Variant A: Desktop (multi-window) platform -> open dialog window
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime && !ForceOverlayDialogs)
        {
            var ownerWindow = parentWindow?.PlatformControl as AvaloniaWindow ?? desktopLifetime.MainWindow;
            if (ownerWindow == null)
                throw new NavigationException("No main window/view available. Application not fully initialized yet.");

            dialog.SetSize(parentWindow);

            var platformWindow = new AvaloniaWindow
            {
                Title = dialog.Title, Icon = parentWindow?.Icon, CanResize = false
            };

            // Dialog windows participate in the template window system: apply the parent window's look,
            // styles and resources before the dialog-specific content and sizing is applied.
            var templateWindow = parentWindow ?? Application.Current?.GetMainWindow();
            if (templateWindow != null)
            {
                ((TemplatedControl) templateWindow).ClonePropertiesTo(platformWindow);

                // The Tag identifies the RouteNav window that owns a platform window. Dialog windows are
                // not owned by a RouteNav window, so the clone's Tag (pointing at the template) is cleared.
                platformWindow.Tag = null;
            }

            platformWindow.Content = dialog;
            platformWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            platformWindow.VerticalContentAlignment = VerticalAlignment.Stretch;
            platformWindow.Width = dialog.Width;
            platformWindow.Height = dialog.Height;
            platformWindow.MinWidth = dialog.Width;
            platformWindow.MinHeight = dialog.Height;

            // Customize to show platform dialog (border style, etc.)
            platformWindow.SetDialogStyle();

            WindowCustomizationEvent?.Invoke(platformWindow, true);
            dialog.RegisterPlatform(platformWindow);
            dialogTask = dialog.Open();

            platformWindow.ShowDialog(ownerWindow);
            return true;
        }

        // Variant B: Mobile/Browser (single window/view) platform + fallback -> open dialog in overlay
        dialogTask = Task.FromCanceled<object?>(CancellationToken.None);
        return false;
    }

    /// <inheritdoc />
    public virtual AvaloniaWindow CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        var platformWindow = new AvaloniaWindow { Title = window.Title, Icon = window.Icon };

        // Host the template window: it joins the visual/logical tree, so window-level XAML
        // (Background/Foreground bindings, styles, resources) resolves natively against the
        // application resource chain and keeps following theme/density changes.
        platformWindow.Tag = window;
        platformWindow.Content = window;

        WindowCustomizationEvent?.Invoke(platformWindow);
        window.RegisterPlatform(desktopLifetime, platformWindow);

        return platformWindow;
    }

    /// <inheritdoc />
    public virtual ContentControl CreatePlatformView(Window window, IApplicationLifetime appLifetime)
    {
        var platformControl = new UserControl
        {
            // Host the template window: it joins the visual/logical tree of the host view, so
            // window-level XAML resolves natively against the application resource chain.
            Tag = window, Content = window
        };

        window.RegisterPlatform(appLifetime, platformControl);

        return platformControl;
    }
}
