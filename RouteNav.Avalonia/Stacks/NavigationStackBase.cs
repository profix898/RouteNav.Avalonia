using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.StackControls;

namespace RouteNav.Avalonia.Stacks;

public abstract class NavigationStackBase<TC> : IPageNavigation, IDialogNavigation, IRouteNavigation, INavigationStack
    where TC : NavigationContainer, new()
{
    protected readonly List<Page> pageStack = new List<Page>();
    protected readonly List<Dialog> dialogStack = new List<Dialog>();
    
    protected NavigationStackBase(string name)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        Name = name;
        BaseUri = new Uri(Navigation.BaseRouteUri, name);
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

        CurrentPage = RootPage = new NotFoundPage();
    }

    public LazyValue<TC> Container { get; }

    protected abstract Page? ResolveRoute(Uri routeUri);

    #region Implementation of INavigationStack

    public virtual string Name { get; }

    public virtual Uri BaseUri { get; }

    public virtual bool IsMainStack => Name.Equals(Navigation.MainStackName);

    public bool IsEventStack => false;

    public event Action? Entered;

    public event Action? Exited;

    public LazyValue<NavigationContainer> ContainerPage => new LazyValue<NavigationContainer>(() => Container.Value);

    public Page RootPage { get; protected set; }

    protected abstract TC InitContainer();

    public virtual INavigationStack? RequestStack(string stackName)
    {
        return null;
    }

    protected virtual Dialog BuildDialog(Page page)
    {
        return page.ToDialog(CurrentPage);
    }

    public virtual void AddPage<TPage>(string relativeRoute)
        where TPage : Page
    {
        AddPage(relativeRoute, typeof(TPage));
    }

    public virtual void AddPage(string relativeRoute, Type pageType)
    {
        if (!pageType.IsSubclassOf(typeof(Page)))
            throw new ArgumentException($"Type '{pageType.FullName}' is not a page.", nameof(pageType));

        AddPage(relativeRoute.TrimStart('/'), () => Navigation.UIPlatform.GetPage(pageType)
                                                    ?? throw new NavigationException($"Page of type '{pageType}' can not be resolved."));
    }

    public abstract void AddPage(string relativeRoute, Func<Page> pageFactory);

    public virtual void Reset()
    {
        pageStack.Clear();

        Exited?.Invoke();

        ContainerPage.Reset();
        Container.Reset();
    }

    #endregion

    #region Implementation of IPageNavigation

    public event Action<NavigationEventArgs<Page>>? PageNavigated;

    protected void OnPageNavigated(Page? pageFrom, Page? pageTo)
    {
        PageNavigated?.Invoke(new NavigationEventArgs<Page>(pageFrom, pageTo));
    }

    public virtual IReadOnlyList<Page> PageStack => pageStack;

    public virtual Page CurrentPage { get; protected set; }

    public virtual void InsertPageBefore(Page page, Page beforePage)
    {
        pageStack.Insert(pageStack.IndexOf(beforePage), page);
    }

    public virtual void RemovePage(Page page)
    {
        if (pageStack.LastOrDefault() == page)
        {
            PopAsync();
            return;
        }

        pageStack.Remove(page);
    }

    public virtual Task PushAsync(Page page)
    {
        if (Equals(page, CurrentPage))
            return Task.CompletedTask;

        var previousPage = CurrentPage;

        pageStack.Add(page);
        CurrentPage = page;

        ContainerPage.Value.UpdatePage(CurrentPage);
        OnPageNavigated(previousPage, CurrentPage);

        return Task.CompletedTask;
    }

    public virtual Task<Page> PopAsync()
    {
        if (pageStack.Count < 1)
            throw new NavigationException("Can't pop page from empty page stack.");

        var previousPage = pageStack.Last();
        pageStack.Remove(previousPage);
        
        var nextPage = pageStack.LastOrDefault();
        if (nextPage == null)
            pageStack.Add(nextPage = RootPage);
        CurrentPage = nextPage;

        ContainerPage.Value.UpdatePage(CurrentPage);
        OnPageNavigated(previousPage, CurrentPage);

        return Task.FromResult(previousPage);
    }

    public virtual Task PopToRootAsync()
    {
        var previousPage = pageStack.Last();
        pageStack.Clear();

        pageStack.Add(RootPage);
        CurrentPage = RootPage;

        ContainerPage.Value.UpdatePage(CurrentPage);
        OnPageNavigated(previousPage, CurrentPage);

        return Task.FromResult(previousPage);
    }

    #endregion

    #region Implementation of IDialogNavigation

    public event Action<NavigationEventArgs<Dialog>>? DialogNavigated;

    protected void OnDialogNavigated(Dialog? dialogFrom, Dialog? dialogTo)
    {
        DialogNavigated?.Invoke(new NavigationEventArgs<Dialog>(dialogFrom, dialogTo));
    }

    public virtual IReadOnlyList<Dialog> DialogStack => dialogStack;

    public virtual Dialog? CurrentDialog { get; protected set; }

    public virtual Task<object?> PushDialogAsync(Dialog dialog)
    {
        var previousDialog = dialogStack.LastOrDefault();

        dialogStack.Add(dialog);
        CurrentDialog = dialog;

        var dialogTask = ContainerPage.Value.UpdateDialog(dialog);
        OnDialogNavigated(previousDialog, dialog);

        return dialogTask;
    }

    public virtual Task<Dialog> PopDialogAsync()
    {
        if (dialogStack.Count < 1)
            throw new NavigationException("Can't pop dialog from empty dialog stack.");

        var previousDialog = dialogStack.Last();
        previousDialog.Close();
        dialogStack.Remove(previousDialog);
        var nextDialog = dialogStack.LastOrDefault();
        CurrentDialog = nextDialog;

        ContainerPage.Value.UpdateDialog(nextDialog);
        OnDialogNavigated(previousDialog, nextDialog);

        return Task.FromResult(previousDialog);
    }

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

    public event Action<NavigationEventArgs<Uri>>? RouteNavigated;

    protected void OnRouteNavigated(Uri? routeFrom, Uri? routeTo)
    {
        RouteNavigated?.Invoke(new NavigationEventArgs<Uri>(routeFrom, routeTo));
    }

    public Task<Page> PushAsync(string relativeRoute, NavigationTarget target = NavigationTarget.Self)
    {
        return PushAsync(this.BuildRoute(relativeRoute), target);
    }

    public virtual async Task<Page> PushAsync(Uri routeUri, NavigationTarget target = NavigationTarget.Self)
    {
        if (routeUri.IsAbsoluteUri && !BaseUri.IsBaseOf(routeUri))
            return await Navigation.PushAsync(routeUri, target);

        var page = ResolveRoute(routeUri) ?? new NotFoundPage();
        
        if (target != NavigationTarget.Dialog)
            await PushAsync(page);
        else
            await PushDialogAsync(BuildDialog(page));

        OnRouteNavigated(null, routeUri);

        return page;
    }

    #endregion
}