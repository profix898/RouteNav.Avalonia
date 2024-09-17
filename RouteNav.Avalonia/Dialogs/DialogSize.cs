namespace RouteNav.Avalonia.Dialogs;

/// <summary>Specifies the size of a dialog.</summary>
public enum DialogSize
{
    /// <summary>Small dialog (default: 0.3 * width/height of window).</summary>
    Small,

    /// <summary>Medium dialog (default: 0.6 * width/height of window).</summary>
    Medium,

    /// <summary>Large dialog (default: 0.9 * width/height of window).</summary>
    Large,

    /// <summary>Custom-sized dialog (uses <c>Width</c> and <c>Height</c> members of <see cref="Dialog"/> to determine size).</summary>
    Custom
}
