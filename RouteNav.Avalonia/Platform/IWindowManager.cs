using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Platform;

public interface IWindowManager
{
    public delegate void WindowCustomizationHandler(AvaloniaWindow window, bool isDialogWindow = false);

    /// <summary>Gets a flag indicating whether the platform supports multiple windows (usually true for desktop platforms).</summary>
    bool SupportsMultiWindow { get; }

    /// <summary>Gets or sets a flag indicating whether the application should enforce single window mode (even if the platform support multiple windows).</summary>
    bool ForceSingleWindow { get; init; }

    /// <summary>Gets or sets a flag indicating whether dialogs should be forced to overlay on top of the current view (even if the platform supports dialog windows).</summary>
    bool ForceOverlayDialogs { get; init; }

    event WindowCustomizationHandler WindowCustomizationEvent;

    bool OpenWindow(Window window, Window? parentWindow = null);

    bool OpenDialog(Dialog dialog, out Task<object?> dialogTask, Window? parentWindow = null);

    AvaloniaWindow CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime desktopLifetime);

    ContentControl CreatePlatformView(Window window, ISingleViewApplicationLifetime singleViewLifetime);
}
