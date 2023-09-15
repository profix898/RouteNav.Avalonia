using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouteNav.Avalonia.Dialogs;

public interface IDialogNavigation
{
    public event Action<NavigationEventArgs<Dialog>> DialogNavigated;

    IReadOnlyList<Dialog> DialogStack { get; }

    Dialog? CurrentDialog { get; }

    Task<object?> PushDialogAsync(Dialog dialog, bool forceOverlay = false);

    Task<Dialog> PopDialogAsync();

    Task PopDialogAllAsync();
}
