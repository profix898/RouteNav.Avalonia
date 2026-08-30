namespace RouteNav.Avalonia;

/// <summary>Window-manager level options, available via <see cref="Navigation.Windows" />.</summary>
public class WindowOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether navigating to a stack that is hosted in a different
    /// window brings that window to the foreground. Defaults to <c>true</c>.
    /// </summary>
    public bool BringTargetWindowToFront { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the application should enforce single window mode
    /// (even if the platform supports multiple windows). Defaults to <c>false</c>.
    /// </summary>
    public bool ForceSingleWindow { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether dialogs should be forced to render as overlays on top
    /// of the current view (even if the platform supports dialog windows). Defaults to <c>false</c>.
    /// </summary>
    public bool ForceOverlayDialogs { get; set; }
}
