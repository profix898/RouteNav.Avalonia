using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

/// <summary>Base implementation for RouteNav navigation stacks.</summary>
/// <typeparam name="TC">The navigation container type used by the stack.</typeparam>
public abstract class NavigationStackBase<TC> : IPageNavigation, IDialogNavigation, IRouteNavigation, INavigationStack
    where TC : NavigationContainer, new()
{
    /// <summary>Stores page registrations by stack-relative route path.</summary>
    protected readonly Dictionary<string, RegisteredRoute> pages = new Dictionary<string, RegisteredRoute>(StringComparer.Ordinal);

    /// <summary>Stores the active page history.</summary>
    protected readonly List<Page> pageStack = new List<Page>();

    /// <summary>Stores the active dialog history.</summary>
    protected readonly List<Dialog> dialogStack = new List<Dialog>();

    /// <summary>Initializes a new instance of the <see cref="NavigationStackBase{TC}" /> class.</summary>
    protected NavigationStackBase(string name, string title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));

        Name = name;
        Title = title;
        isMainStack = name.Equals(Navigation.MainStackName, StringComparison.Ordinal);
        BaseUri = new Uri(Navigation.BaseRouteUri, name);
        BaseUriString = BaseUri.AbsoluteUri;
        Container = new LazyValue<TC>(() =>
        {
            try
            {
                return InitContainer();
            }
            finally
            {
                Entered?.Invoke();
            }
        });
    }

    /// <summary>Gets the lazily-created typed container for this stack.</summary>
    protected LazyValue<TC> Container { get; }

    /// <summary>Resolves a route to a page using the dynamic resolver first, then registered page factories.</summary>
    protected virtual Page? ResolveRoute(Uri routeUri)
    {
        var page = PageResolver?.ResolveRoute(routeUri);
        if (page != null)
            return page;

        // Zero-allocation route path lookup (span-based dictionary key)
        var lookup = pages.GetAlternateLookup<ReadOnlySpan<char>>();
        return lookup.TryGetValue(this.GetRoutePathSpan(routeUri), out var registration)
            ? registration.PageFactory?.Invoke(routeUri)
            : null;
    }

    #region Implementation of INavigationStack

    /// <inheritdoc />
    public virtual string Name { get; }

    /// <inheritdoc />
    public virtual string Title { get; }

    /// <inheritdoc />
    public virtual Uri BaseUri { get; }

    /// <inheritdoc />
    public virtual string BaseUriString { get; }

    /// <inheritdoc />
    public virtual bool IsMainStack => isMainStack;

    /// <summary>Cached main-stack check (the main stack name is set before stacks are registered).</summary>
    private readonly bool isMainStack;

    /// <inheritdoc />
    public bool IsEventStack => false;

    /// <inheritdoc />
    public WindowFactory? WindowFactory { get; set; }

    /// <inheritdoc />
    public event Action? Entered;

    /// <inheritdoc />
    public event Action? Exited;

    private LazyValue<NavigationContainer>? containerPage;

    /// <summary>Gets the lazily-created navigation container that hosts this stack's pages.</summary>
    public LazyValue<NavigationContainer> ContainerPage => containerPage ??= new LazyValue<NavigationContainer>(() => Container.Value);

    /// <summary>Gets the lazily-created root (initial) page of this stack.</summary>
    public LazyValue<Page> RootPage { get; protected set; } = null!;

    /// <inheritdoc />
    public IPageResolver? PageResolver { get; set; }

    /// <summary>Creates and initializes the stack container.</summary>
    protected abstract TC InitContainer();

    /// <summary>Builds a dialog wrapper for a page pushed as a dialog.</summary>
    protected virtual Dialog BuildDialog(Page page)
    {
        return page.ToDialog(CurrentPage);
    }

    /// <inheritdoc />
    public virtual void AddPage(string relativeRoute, Type pageType)
    {
        if (!pageType.IsSubclassOf(typeof(Page)))
            throw new ArgumentException($"Type '{pageType.FullName}' is not a page.", nameof(pageType));

        var pageKey = relativeRoute.Trim('/');
        pages.Set(pageKey, new RegisteredRoute(pageKey, pageType,
            uri => Navigation.UIPlatform.GetPage(pageType, uri)
                   ?? throw new NavigationException($"Page of type '{pageType}' can not be resolved.")));
    }

    /// <inheritdoc />
    public virtual void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
    {
        var pageKey = relativeRoute.Trim('/');
        pages.Set(pageKey, new RegisteredRoute(pageKey, null, pageFactory));
    }

    /// <inheritdoc />
    public IReadOnlyList<RegisteredRoute> RegisteredRoutes => registeredRoutes ??= new RegisteredRouteCollection(pages);

    private RegisteredRouteCollection? registeredRoutes;

    /// <inheritdoc />
    public virtual INavigationStack? RequestStack(string stackName)
    {
        return null;
    }

    /// <inheritdoc />
    public virtual void Reset()
    {
        pageStack.Clear();

        Exited?.Invoke();

        containerPage?.Reset();
        Container.Reset();
        RootPage?.Reset();
        CurrentPage = null;
    }

    #endregion

    #region Implementation of IPageNavigation

    /// <inheritdoc />
    public event Action<NavigationEventArgs<Page>>? PageNavigated;

    /// <summary>Raises <see cref="PageNavigated" />.</summary>
    protected void OnPageNavigated(Page? pageFrom, Page? pageTo)
    {
        PageNavigated?.Invoke(new NavigationEventArgs<Page>(pageFrom, pageTo));
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<Page> PageStack => pageStack;

    /// <inheritdoc />
    public virtual Page? CurrentPage { get; protected set; }

    /// <inheritdoc />
    public virtual void InsertPageBefore(Page page, Page beforePage)
    {
        pageStack.Insert(pageStack.IndexOf(beforePage), page);
    }

    /// <inheritdoc />
    public virtual void RemovePage(Page page)
    {
        if (pageStack.LastOrDefault() == page)
        {
            PopAsync();
            return;
        }

        pageStack.Remove(page);
    }

    /// <inheritdoc />
    public virtual Task<Page> PushAsync(Page page)
    {
        if (page.Equals(CurrentPage))
            return Task.FromResult(CurrentPage);

        var previousPage = CurrentPage;

        pageStack.Add(page);
        CurrentPage = page;

        ContainerPage.Value.UpdatePage(CurrentPage);
        OnPageNavigated(previousPage, CurrentPage);

        return Task.FromResult(CurrentPage);
    }

    /// <inheritdoc />
    public virtual Task<Page> PopAsync()
    {
        if (pageStack.Count < 1)
            throw new NavigationException("Can't pop page from empty page stack.");

        var previousPage = pageStack.Last();
        pageStack.Remove(previousPage);

        var nextPage = pageStack.LastOrDefault();
        if (nextPage == null && IsMainStack)
            pageStack.Add(nextPage = RootPage.Value); // Default to RootPage for MainStack
        CurrentPage = nextPage;

        if (CurrentPage != null)
            ContainerPage.Value.UpdatePage(CurrentPage);
        OnPageNavigated(previousPage, CurrentPage);

        if (CurrentPage == null)
            Navigation.PopAsync(this);

        return Task.FromResult(previousPage);
    }

    /// <inheritdoc />
    public virtual Task PopToRootAsync()
    {
        var previousPage = pageStack.Last();
        pageStack.Clear();

        pageStack.Add(RootPage.Value);
        CurrentPage = RootPage.Value;

        ContainerPage.Value.UpdatePage(CurrentPage);
        OnPageNavigated(previousPage, CurrentPage);

        return Task.FromResult(previousPage);
    }

    #endregion

    #region Implementation of IDialogNavigation

    /// <inheritdoc />
    public event Action<NavigationEventArgs<Dialog>>? DialogNavigated;

    /// <summary>Raises <see cref="DialogNavigated" />.</summary>
    protected void OnDialogNavigated(Dialog? dialogFrom, Dialog? dialogTo)
    {
        DialogNavigated?.Invoke(new NavigationEventArgs<Dialog>(dialogFrom, dialogTo));
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<Dialog> DialogStack => dialogStack;

    /// <inheritdoc />
    public virtual Dialog? CurrentDialog { get; protected set; }

    /// <inheritdoc />
    public virtual Task<object?> PushDialogAsync(Dialog dialog, bool forceOverlay = false)
    {
        var previousDialog = dialogStack.LastOrDefault();

        dialogStack.Add(dialog);
        CurrentDialog = dialog;

        var dialogTask = ContainerPage.Value.UpdateDialog(dialog, forceOverlay);
        dialog.Closed += (_, _) => { dialogStack.Remove(dialog); };
        OnDialogNavigated(previousDialog, dialog);

        return dialogTask;
    }

    /// <inheritdoc />
    public virtual Task<Dialog> PopDialogAsync()
    {
        if (dialogStack.Count < 1)
            throw new NavigationException("Can't pop dialog from empty dialog stack.");

        var previousDialog = dialogStack.Last();
        previousDialog.Close();

        //dialogStack.Remove(previousDialog);
        var nextDialog = dialogStack.LastOrDefault();
        CurrentDialog = nextDialog;

        ContainerPage.Value.UpdateDialog(nextDialog);
        OnDialogNavigated(previousDialog, nextDialog);

        return Task.FromResult(previousDialog);
    }

    /// <inheritdoc />
    public virtual Task PopDialogAllAsync()
    {
        var previousDialog = dialogStack.LastOrDefault();
        foreach (var dialog in dialogStack)
            dialog.Close();
        dialogStack.Clear();
        CurrentDialog = null;

        ContainerPage.Value.UpdateDialog(null);
        OnDialogNavigated(previousDialog, null);

        return Task.FromResult(previousDialog);
    }

    #endregion

    #region Implementation of IRouteNavigation

    /// <inheritdoc />
    public event Action<NavigationEventArgs<Uri>>? RouteNavigated;

    /// <summary>Raises <see cref="RouteNavigated" />.</summary>
    protected void OnRouteNavigated(Uri? routeFrom, Uri? routeTo)
    {
        RouteNavigated?.Invoke(new NavigationEventArgs<Uri>(routeFrom, routeTo));
    }

    /// <inheritdoc />
    public Task<Page> PushAsync(string relativeRoute, NavigationTarget target = NavigationTarget.Self)
    {
        return PushAsync(this.BuildRoute(relativeRoute), target);
    }

    /// <inheritdoc />
    public virtual async Task<Page> PushAsync(Uri routeUri, NavigationTarget target = NavigationTarget.Self)
    {
        if (routeUri.IsAbsoluteUri && !BaseUri.IsBaseOf(routeUri))
            return await Navigation.PushAsync(routeUri, target);

        var page = ResolveRoute(routeUri) ?? new NotFoundPage();

        if (target != NavigationTarget.Dialog && target != NavigationTarget.DialogOverlay)
            await PushAsync(page);
        else
            await PushDialogAsync(BuildDialog(page), target == NavigationTarget.DialogOverlay);

        OnRouteNavigated(null, routeUri);

        return page;
    }

    #endregion
}
