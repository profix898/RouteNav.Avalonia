using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

/// <summary>
/// A stack that delegates route handling to an event instead of maintaining its own page stack.
/// </summary>
public class RouteEventStack : IPageNavigation, IDialogNavigation, IRouteNavigation, INavigationStack
{
    /// <summary>Initializes a new instance of the <see cref="RouteEventStack" /> class.</summary>
    public RouteEventStack(string name, Func<Uri, Page?>? eventHandler = null)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        Name = name;
        Title = "EventStack";
        BaseUri = new Uri(Navigation.BaseRouteUri, name);
        BaseUriString = BaseUri.AbsoluteUri;

        if (eventHandler != null)
            RouteEvent += eventHandler;
    }

    /// <summary>Raised when this stack receives a route; return a page to show it as a dialog on the main stack.</summary>
    public event Func<Uri, Page?>? RouteEvent;

    #region Implementation of INavigationStack

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public Uri BaseUri { get; }

    /// <inheritdoc />
    public string BaseUriString { get; }

    /// <inheritdoc />
    public bool IsMainStack => false;

    /// <inheritdoc />
    public bool IsEventStack => true;

    /// <inheritdoc />
    public WindowFactory? WindowFactory { get; set; }

    /// <summary>Event 'Entered' not supported for event stack.</summary>
    public event Action? Entered;

    /// <summary>Event 'Exited' not supported for event stack.</summary>
    public event Action? Exited;

    /// <inheritdoc />
    public LazyValue<NavigationContainer> ContainerPage => new LazyValue<NavigationContainer>(() => new NavigationContainer { Content = RootPage });

    /// <inheritdoc />
    public LazyValue<Page> RootPage { get; } = new LazyValue<Page>(() => new Page());

    /// <inheritdoc />
    public IPageResolver PageResolver
    {
        get { throw new NotImplementedException($"{typeof(RouteEventStack)} does not support an {typeof(IPageResolver)}."); }
        set { throw new NotImplementedException($"{typeof(RouteEventStack)} does not support an {typeof(IPageResolver)}."); }
    }

    /// <inheritdoc />
    public Page? CurrentPage => RootPage.Value;

    /// <inheritdoc />
    public INavigationStack? RequestStack(string stackName)
    {
        return null;
    }

    /// <inheritdoc />
    public void AddPage(string relativeRoute, Type pageType)
    {
        throw new NotSupportedException($"{nameof(RouteEventStack)} does not support pages.");
    }

    /// <inheritdoc />
    public void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
    {
        throw new NotSupportedException($"{nameof(RouteEventStack)} does not support pages.");
    }

    /// <summary>Event stacks do not register pages -> always empty.</summary>
    public IReadOnlyList<RegisteredRoute> RegisteredRoutes => EmptyRoutes;

    private static readonly IReadOnlyList<RegisteredRoute> EmptyRoutes = Array.Empty<RegisteredRoute>();

    /// <inheritdoc />
    public void Reset()
    {
    }

    #endregion

    #region Implementation of IPageNavigation

    /// <inheritdoc />
    public IReadOnlyList<Page> PageStack { get; } = new List<Page>();

    /// <summary>Event 'PageNavigated' not supported for event stack.</summary>
    public event Action<NavigationEventArgs<Page>>? PageNavigated;

    /// <inheritdoc />
    public void InsertPageBefore(Page page, Page beforePage)
    {
    }

    /// <inheritdoc />
    public void RemovePage(Page page)
    {
    }

    /// <inheritdoc />
    public Task<Page> PushAsync(Page page)
    {
        return Task.FromException<Page>(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IPageNavigation."));
    }

    /// <inheritdoc />
    public Task<Page> PopAsync()
    {
        return Task.FromException<Page>(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IPageNavigation."));
    }

    /// <inheritdoc />
    public Task PopToRootAsync()
    {
        return Task.FromException(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IPageNavigation."));
    }

    #endregion

    #region Implementation of IDialogNavigation

    /// <inheritdoc />
    public IReadOnlyList<Dialog> DialogStack { get; } = new List<Dialog>();

    /// <inheritdoc />
    public Dialog? CurrentDialog { get; } = null;

    /// <summary>Event 'DialogNavigated' not supported for event stack.</summary>
    public event Action<NavigationEventArgs<Dialog>>? DialogNavigated;

    /// <inheritdoc />
    public Task<object?> PushDialogAsync(Dialog dialog, bool forceOverlay = false)
    {
        return Task.FromException<object?>(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IDialogNavigation."));
    }

    /// <inheritdoc />
    public Task<Dialog> PopDialogAsync()
    {
        return Task.FromException<Dialog>(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IDialogNavigation."));
    }

    /// <inheritdoc />
    public Task PopDialogAllAsync()
    {
        return Task.FromException<Dialog>(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IDialogNavigation."));
    }

    #endregion

    #region Implementation of IRouteNavigation

    /// <summary>Event 'RouteNavigated' not supported for event stack.</summary>
    public event Action<NavigationEventArgs<Uri>>? RouteNavigated;

    /// <inheritdoc />
    public Task<Page> PushAsync(string relativeRoute, NavigationTarget target = NavigationTarget.Self)
    {
        return PushAsync(this.BuildRoute(relativeRoute), target);
    }

    /// <inheritdoc />
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
                await Navigation.GetMainStack().PushDialogAsync(page, forceOverlay: target == NavigationTarget.DialogOverlay);

                return page;
            });
        }

        return RootPage.Value;
    }

    #endregion
}
