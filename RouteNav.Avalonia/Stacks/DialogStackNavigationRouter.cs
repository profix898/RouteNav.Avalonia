using System;
using System.Threading.Tasks;
using NSE.RouteNav.Controls;

namespace NSE.RouteNav.Stacks;

public class DialogStackNavigationRouter : INavigationRouter
{
    private readonly INavigationStack navigationStack;

    public DialogStackNavigationRouter(INavigationStack navigationStack)
    {
        this.navigationStack = navigationStack;

        navigationStack.DialogNavigated += args => Navigated?.Invoke(this, new NavigatedEventArgs(args.dialogFrom, args.dialogTo));
    }

    #region Implementation of INavigationRouter

    /// <inheritdoc />
    public event EventHandler<NavigatedEventArgs>? Navigated;

    /// <inheritdoc />
    public bool AllowEmpty
    {
        get { return false; }
        set { }
    }

    /// <inheritdoc />
    public bool CanGoBack => navigationStack.DialogStack.Count > 1;

    /// <inheritdoc />
    public object? CurrentPage { get; private set; }

    /// <inheritdoc />
    public Task ForwardAsync()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public bool CanGoForward => false;

    /// <inheritdoc />
    public async Task NavigateToAsync(object? destination)
    {
        if (destination is Dialog dialog)
            await navigationStack.PushDialogAsync(dialog);
        else
            throw new ArgumentException($"{nameof(NavigateToAsync)} supports navigation for {nameof(Dialog)} objects only.", nameof(destination));
    }

    /// <inheritdoc />
    public async Task BackAsync()
    {
        await navigationStack.PopDialogAsync();
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        await navigationStack.PopDialogAllAsync();
    }

    #endregion
}
