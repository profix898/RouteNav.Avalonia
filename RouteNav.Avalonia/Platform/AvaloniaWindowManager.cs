using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Platform;

public class AvaloniaWindowManager : IWindowManager
{
    public bool ForceSingleWindow { get; set; }

    public IApplicationLifetime? ApplicationLifetime => Application.Current!.ApplicationLifetime;

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

    public virtual Task<object> OpenDialog(Dialogs.Dialog dialog, Window? parentWindow = null)
    {
        // Variant A: Desktop (multi-window) platform -> open dialog window
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime && !ForceSingleWindow)
        {
            var platformWindow = new AvaloniaWindow
            {
                Title = dialog.Title,

                Content = dialog.Content,
                Width = dialog.Size.Width,
                Height = dialog.Size.Height,
                MinWidth = dialog.Size.Width,
                MinHeight = dialog.Size.Height,

                CanResize = false
            };

            // TODO: Customize to show platform dialog (border style, etc.)
            
            var ownerWindow = parentWindow?.PlatformControl as AvaloniaWindow ?? desktopLifetime.MainWindow;
            if (ownerWindow == null)
                throw new NavigationException("No main window/view available. Application not fully initialized yet.");

            return platformWindow.ShowDialog<object>(ownerWindow);
        }

        // Variant B: Mobile/Browser (single window/view) platform + Fallback -> open dialog in overlay
        return dialog.ShowDialog();
    }

    public virtual AvaloniaWindow CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime desktopLifetime, Action<AvaloniaWindow>? windowCustomization = null)
    {
        var platformWindow = new AvaloniaWindow // Clone into AvaloniaWindow
        {
            Title = window.Title,
            Icon = window.Icon,

            // ContentControl
            Content = window.Content,
            ContentTemplate = window.ContentTemplate,
            HorizontalContentAlignment = window.HorizontalContentAlignment,
            VerticalContentAlignment = window.VerticalContentAlignment,

            // TemplatedControl
            Background = window.Background,
            BorderBrush = window.BorderBrush,
            BorderThickness = window.BorderThickness,
            CornerRadius = window.CornerRadius,
            FontFamily = window.FontFamily,
            FontSize = window.FontSize,
            FontStyle = window.FontStyle,
            FontWeight = window.FontWeight,
            FontStretch = window.FontStretch,
            Foreground = window.Foreground,
            Padding = window.Padding,

            // Control
            FocusAdorner = window.FocusAdorner,
            Tag = window,
            ContextMenu = window.ContextMenu,
            ContextFlyout = window.ContextFlyout
        };
#if DEBUG
        platformWindow.AttachDevTools();
#endif
        windowCustomization?.Invoke(platformWindow);

        window.RegisterPlatform(desktopLifetime, platformWindow);

        return platformWindow;
    }

    public virtual ContentControl CreatePlatformView(Window window, ISingleViewApplicationLifetime singleViewLifetime)
    {
        var platformControl = new ContentControl // Clone into new ContentControl
        {
            // ContentControl
            Content = window.Content,
            ContentTemplate = window.ContentTemplate,
            HorizontalContentAlignment = window.HorizontalContentAlignment,
            VerticalContentAlignment = window.VerticalContentAlignment,

            // TemplatedControl
            Background = window.Background,
            BorderBrush = window.BorderBrush,
            BorderThickness = window.BorderThickness,
            CornerRadius = window.CornerRadius,
            FontFamily = window.FontFamily,
            FontSize = window.FontSize,
            FontStyle = window.FontStyle,
            FontWeight = window.FontWeight,
            FontStretch = window.FontStretch,
            Foreground = window.Foreground,
            Padding = window.Padding,

            // Control
            FocusAdorner = window.FocusAdorner,
            Tag = window,
            ContextMenu = window.ContextMenu,
            ContextFlyout = window.ContextFlyout
        };

        window.RegisterPlatform(singleViewLifetime, platformControl);

        return platformControl;
    }
}
