namespace RouteNav.Avalonia.Dialogs;

/// <summary>Specifies how a dialog hosts its content.</summary>
public enum DialogContentMode
{
    /// <summary>Content is hosted in a scroll viewer whose height is constrained to the dialog frame.</summary>
    Scroll,

    /// <summary>Content is hosted without height constraints; the dialog frame grows and shrinks with the content.</summary>
    Wrap
}
