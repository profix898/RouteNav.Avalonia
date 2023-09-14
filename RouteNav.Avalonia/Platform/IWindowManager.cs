using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Platform;

public interface IWindowManager
{
    public delegate void WindowCustomizationHandler(AvaloniaWindow window, bool isDialogWindow = false);

    bool SupportsMultiWindow { get; }

    bool ForceSingleWindow { get; init; }

    bool ForceOverlayDialogs { get; init; }

    event WindowCustomizationHandler WindowCustomizationEvent;

    bool OpenWindow(Window window, Window? parentWindow = null);

    bool OpenDialog(Dialog dialog, out Task<object?> dialogTask, Window? parentWindow = null);

    AvaloniaWindow CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime desktopLifetime);

    ContentControl CreatePlatformView(Window window, ISingleViewApplicationLifetime singleViewLifetime);
}
