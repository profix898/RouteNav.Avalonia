namespace RouteNav.Avalonia.Dialogs;

/// <summary>The result returned by a <see cref="MessageDialog"/>.</summary>
public enum MessageDialogResult
{
    /// <summary>No result (dialog dismissed without a choice).</summary>
    None,
    /// <summary>The OK button was chosen.</summary>
    Ok,
    /// <summary>The Cancel button was chosen.</summary>
    Cancel,
    /// <summary>The Yes button was chosen.</summary>
    Yes,
    /// <summary>The No button was chosen.</summary>
    No
}
