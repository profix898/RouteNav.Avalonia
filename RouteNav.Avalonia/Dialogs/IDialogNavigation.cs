using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSE.RouteNav.Dialogs;

public interface IDialogNavigation
{
    IReadOnlyList<Dialog> DialogStack { get; }

    public event Action<(Dialog? dialogFrom, Dialog? dialogTo)> DialogNavigated;

    Task PushDialogAsync(Dialog dialog);

    Task<Dialog> PopDialogAsync();

    Task PopDialogAllAsync();
}
