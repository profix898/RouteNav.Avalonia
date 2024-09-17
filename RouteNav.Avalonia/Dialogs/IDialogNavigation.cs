using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouteNav.Avalonia.Dialogs;

/// <summary>Defines the contract for dialog navigation (typically as modal window or overlay dialog).</summary>
public interface IDialogNavigation
{
    /// <summary>Occurs when a dialog has been navigated (from Dialog -> to Dialog).</summary>
    public event Action<NavigationEventArgs<Dialog>> DialogNavigated;

    /// <summary>Gets the current stack of dialogs.</summary>
    IReadOnlyList<Dialog> DialogStack { get; }

    /// <summary>Gets the current dialog.</summary>
    Dialog? CurrentDialog { get; }

    /// <summary>Pushes a dialog onto the stack.</summary>
    /// <param name="dialog">Dialog to be pushed onto the stack.</param>
    /// <param name="forceOverlay">If true, forces the dialog to be displayed as an overlay.</param>
    /// <returns>Task with the new dialog as the result.</returns>
    Task<object?> PushDialogAsync(Dialog dialog, bool forceOverlay = false);

    /// <summary>Pops the last dialog from the stack.</summary>
    /// <returns>Task with the new dialog as the result.</returns>
    Task<Dialog> PopDialogAsync();

    /// <summary>Pops all dialogs from the stack.</summary>
    Task PopDialogAllAsync();
}
