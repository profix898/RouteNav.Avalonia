using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using NSE.RouteNav.Dialogs;
using NSE.RouteNav.Pages;
using NSE.RouteNav.Routes;
using NSE.RouteNav.Stacks.Internal;

namespace NSE.RouteNav.Stacks;

public class RouteEventStack : IPageNavigation, IDialogNavigation, IRouteNavigation, INavigationStack
{
    public RouteEventStack(string name, Func<Uri, Page?>? eventHandler = null)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        Name = name;
        BaseUri = new Uri(Navigation.BaseRouteUri, name);

        if (eventHandler != null)
            RouteEvent += eventHandler;
    }

    public event Func<Uri, Page?>? RouteEvent;

    #region Implementation of INavigationStack

    public string Name { get; }

    public Uri BaseUri { get; }

    public bool IsMainStack => false;

    public bool IsEventStack => true;

    /// <summary>Event 'Entered' not supported for event stack.</summary>
    public event Action? Entered;

    /// <summary>Event 'Exited' not supported for event stack.</summary>
    public event Action? Exited;

    public LazyValue<ContentControl> ContainerPage => new LazyValue<ContentControl>(() => RootPage);

    public Page RootPage { get; } = new Page();

    public Page CurrentPage => RootPage;

    public INavigationStack? RequestStack(string stackName)
    {
        return null;
    }

    public void AddPage(string relativeRoute, Func<Page> pageFactory)
    {
        throw new NotSupportedException($"{nameof(RouteEventStack)} does not support pages.");
    }

    public void AddPage(string relativeRoute, Type pageType)
    {
        throw new NotSupportedException($"{nameof(RouteEventStack)} does not support pages.");
    }

    public void Reset()
    {
    }

    #endregion

    #region Implementation of IPageNavigation

    public IReadOnlyList<Page> PageStack { get; } = new List<Page>();

    /// <summary>Event 'PageNavigated' not supported for event stack.</summary>
    public event Action<(Page? pageFrom, Page? pageTo)>? PageNavigated;

    public void InsertPageBefore(Page page, Page beforePage)
    {
    }

    public void RemovePage(Page page)
    {
    }

    public Task PushAsync(Page page)
    {
        return Task.FromException(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IPageNavigation."));
    }

    public Task<Page> PopAsync()
    {
        return Task.FromException<Page>(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IPageNavigation."));
    }

    public Task PopToRootAsync()
    {
        return Task.FromException(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IPageNavigation."));
    }

    #endregion

    #region Implementation of IDialogNavigation

    public IReadOnlyList<Dialog> DialogStack { get; } = new List<Dialog>();

    /// <summary>Event 'DialogNavigated' not supported for event stack.</summary>
    public event Action<(Dialog? dialogFrom, Dialog? dialogTo)>? DialogNavigated;

    public Task PushDialogAsync(Dialog dialog)
    {
        return Task.FromException<Page>(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IDialogNavigation."));
    }

    public Task<Dialog> PopDialogAsync()
    {
        return Task.FromException<Dialog>(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IDialogNavigation."));
    }

    public Task PopDialogAllAsync()
    {
        return Task.FromException<Dialog>(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IDialogNavigation."));
    }

    #endregion

    #region Implementation of IRouteNavigation

    /// <summary>Event 'RouteNavigated' not supported for event stack.</summary>
    public event Action<(Uri? routeFrom, Uri? routeTo)>? RouteNavigated;

    public Task<Page> PushAsync(string relativeRoute, NavigationTarget target = NavigationTarget.Self)
    {
        return PushAsync(this.BuildRoute(relativeRoute), target);
    }

    public async Task<Page> PushAsync(Uri routeUri, NavigationTarget target = NavigationTarget.Self)
    {
        if (routeUri.IsAbsoluteUri && !BaseUri.IsBaseOf(routeUri))
            return await Navigation.PushAsync(routeUri, target);

        // Invoke RouteEvent handlers
        var page = RouteEvent?.Invoke(routeUri);
        if (page != null)
        {
            // Show result page in popup view
            return await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Navigation.GetMainStack().PushDialogAsync(page);

                return page;
            });
        }

        return RootPage;
    }

    #endregion
}