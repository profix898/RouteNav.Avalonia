using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.StackControls;

namespace RouteNav.Avalonia.Stacks;

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

    public LazyValue<NavigationContainer> ContainerPage => new LazyValue<NavigationContainer>(() => new NavigationContainer { Content = RootPage });

    public Page RootPage { get; } = new Page();

    public Page? CurrentPage => RootPage;

    public INavigationStack? RequestStack(string stackName)
    {
        return null;
    }

    public void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
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
    public event Action<NavigationEventArgs<Page>>? PageNavigated;

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

    public Dialog? CurrentDialog { get; } = null;

    /// <summary>Event 'DialogNavigated' not supported for event stack.</summary>
    public event Action<NavigationEventArgs<Dialog>>? DialogNavigated;

    public Task<object?> PushDialogAsync(Dialog dialog)
    {
        return Task.FromException<object?>(new NotSupportedException($"{nameof(RouteEventStack)} does not implement IDialogNavigation."));
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
    public event Action<NavigationEventArgs<Uri>>? RouteNavigated;

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