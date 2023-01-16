using System;
using System.Threading.Tasks;
using NSE.RouteNav.Controls;

namespace NSE.RouteNav.Stacks;

public class PageStackNavigationRouter : INavigationRouter
{
    private readonly INavigationStack navigationStack;

    public PageStackNavigationRouter(INavigationStack navigationStack)
    {
        this.navigationStack = navigationStack;

        navigationStack.PageNavigated += args => Navigated?.Invoke(this, new NavigatedEventArgs(args.pageFrom, args.pageTo));
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
    public bool CanGoBack => navigationStack.PageStack.Count > 1;

    /// <inheritdoc />
    public object? CurrentPage => navigationStack.CurrentPage;

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
        if (destination is Page page)
            await navigationStack.PushAsync(page);
        else
            throw new ArgumentException($"{nameof(NavigateToAsync)} supports navigation for {nameof(Page)} objects only.", nameof(destination));
    }

    /// <inheritdoc />
    public async Task BackAsync()
    {
        await navigationStack.PopAsync();
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        await navigationStack.PopToRootAsync();
    }

    #endregion
}
