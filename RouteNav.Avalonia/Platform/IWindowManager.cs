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

    /// <summary>Raised for each platform window created, allowing callers to customize it.</summary>
    event WindowCustomizationHandler WindowCustomizationEvent;

    /// <summary>Opens the given window. The built-in manager shows navigation windows unowned (so they can be raised independently during cross-window navigation); custom managers may use <paramref name="parentWindow" />. Returns <c>false</c> if unsupported.</summary>
    bool OpenWindow(Window window, Window? parentWindow = null);

    /// <summary>Opens the given dialog as a window. Returns <c>false</c> if dialog windows are unsupported (the assigned result task should be ignored by the caller).</summary>
    bool OpenDialog(Dialog dialog, out Task<object?> dialogTask, Window? parentWindow = null);

    /// <summary>Creates the backing platform window for the desktop lifetime.</summary>
    AvaloniaWindow CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime desktopLifetime, bool isDialogWindow = false);

    /// <summary>Creates the backing platform view (single-view / activity lifetimes).</summary>
    ContentControl CreatePlatformView(Window window, IApplicationLifetime appLifetime);
}
