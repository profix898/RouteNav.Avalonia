using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Platform;

/// <summary>
/// Abstracts creation and opening of platform windows, views and dialogs across desktop, mobile and browser.
/// </summary>
public interface IWindowManager
{
    /// <summary>Handler used to customize a platform window right after it is created.</summary>
    public delegate void WindowCustomizationHandler(AvaloniaWindow window, bool isDialogWindow = false);

    /// <summary>Gets a flag indicating whether the platform supports multiple windows (usually true for desktop platforms).</summary>
    bool SupportsMultiWindow { get; }

    /// <summary>Gets or sets a flag indicating whether the application should enforce single window mode (even if the platform support multiple windows).</summary>
    bool ForceSingleWindow { get; init; }

    /// <summary>Gets or sets a flag indicating whether dialogs should be forced to overlay on top of the current view (even if the platform supports dialog windows).</summary>
    bool ForceOverlayDialogs { get; init; }

    /// <summary>Raised for each platform window created, allowing callers to customize it.</summary>
    event WindowCustomizationHandler WindowCustomizationEvent;

    /// <summary>Opens the given window (optionally owned by <paramref name="parentWindow" />). Returns <c>false</c> if unsupported.</summary>
    bool OpenWindow(Window window, Window? parentWindow = null);

    /// <summary>Opens the given dialog as a window. Returns <c>false</c> (and no task) if dialog windows are unsupported.</summary>
    bool OpenDialog(Dialog dialog, out Task<object?> dialogTask, Window? parentWindow = null);

    /// <summary>Creates the backing platform window for the desktop lifetime.</summary>
    AvaloniaWindow CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime desktopLifetime);

    /// <summary>Creates the backing platform view (single-view / activity lifetimes).</summary>
    ContentControl CreatePlatformView(Window window, IApplicationLifetime appLifetime);
}
