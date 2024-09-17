namespace RouteNav.Avalonia.Stacks;

/// <summary>Specifies the preferred target for navigation (may be ignored by the navigation stack implementation).</summary>
public enum NavigationTarget
{
    /// <summary>Opens the route in the current context (i.e. the same stack container).</summary>
    Self,
    /// <summary>Opens the route in the parent context (i.e. the same window).</summary>
    Parent,
    /// <summary>Opens the route in a dialog (associated with the related stack).</summary>
    Dialog,
    /// <summary>Opens the route in an overlay dialog (on top of the related stack).</summary>
    DialogOverlay,
    /// <summary>Opens the route in a new window (potentially switching to the related stack).</summary>
    Window
}
