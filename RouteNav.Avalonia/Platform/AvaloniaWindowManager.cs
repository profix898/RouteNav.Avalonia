using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Internal;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Platform;

public class AvaloniaWindowManager : IWindowManager
{
    private static IApplicationLifetime? ApplicationLifetime => Application.Current!.ApplicationLifetime;

    public virtual bool SupportsMultiWindow => ApplicationLifetime is IClassicDesktopStyleApplicationLifetime;

    public bool ForceSingleWindow { get; init; }

    public bool ForceOverlayDialogs { get; init; }

    public event IWindowManager.WindowCustomizationHandler WindowCustomizationEvent;

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
                Title = dialog.Title,
                Icon = parentWindow?.Icon,

                // ContentControl
                Content = dialog,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,

                Width = dialog.Width,
                Height = dialog.Height,
                MinWidth = dialog.Width,
                MinHeight = dialog.Height,
                CanResize = false,
            };

            // Customize to show platform dialog (border style, etc.)
            platformWindow.SetDialogStyle();
            
            WindowCustomizationEvent?.Invoke(platformWindow, true);
#if DEBUG
            platformWindow.AttachDevTools();
#endif
            dialog.RegisterPlatform(platformWindow);
            dialogTask = dialog.Open();

            platformWindow.ShowDialog(ownerWindow);
            return true;
        }

        // Variant B: Mobile/Browser (single window/view) platform + fallback -> open dialog in overlay
        dialogTask = Task.FromCanceled<object?>(CancellationToken.None);
        return false;
    }

    public virtual AvaloniaWindow CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        var platformWindow = new AvaloniaWindow
        {
            Title = window.Title,
            Icon = window.Icon
        };
        window.ClonePropertiesTo(platformWindow);

        WindowCustomizationEvent?.Invoke(platformWindow, false);
#if DEBUG
        platformWindow.AttachDevTools();
#endif
        window.RegisterPlatform(desktopLifetime, platformWindow);

        return platformWindow;
    }

    public virtual ContentControl CreatePlatformView(Window window, ISingleViewApplicationLifetime singleViewLifetime)
    {
        var platformControl = new UserControl();
        window.ClonePropertiesTo(platformControl);

        window.RegisterPlatform(singleViewLifetime, platformControl);

        return platformControl;
    }
}