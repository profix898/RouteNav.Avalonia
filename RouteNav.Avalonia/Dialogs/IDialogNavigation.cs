using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouteNav.Avalonia.Dialogs;

public interface IDialogNavigation
{
    public event Action<(Dialog? dialogFrom, Dialog? dialogTo)> DialogNavigated;

    IReadOnlyList<Dialog> DialogStack { get; }

    Dialog? CurrentDialog { get; }

    Task PushDialogAsync(Dialog dialog);

    Task<Dialog> PopDialogAsync();

    Task PopDialogAllAsync();
}
