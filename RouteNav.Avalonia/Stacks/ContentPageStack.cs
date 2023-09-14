using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Layout;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Platform;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

public class ContentPageStack : NavigationStackBase<NavigationContainer>, INavigationStack
{
    public ContentPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }

    private Dictionary<string, Func<Uri, Page>> Pages { get; } = new Dictionary<string, Func<Uri, Page>>();

    #region Overrides of NavigationStackBase<NavigationPage>

    protected override Page? ResolveRoute(Uri routeUri)
    {
        return Pages.TryGetValue(this.GetRoutePath(routeUri), out var pageFactory) ? pageFactory(routeUri) : null;
    }

    protected override NavigationContainer InitContainer()
    {
        return new NavigationContainer
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            NavigationStack = this
        };
    }

    public override void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
    {
        var pageKey = relativeRoute.Trim('/');
        Pages.Set(pageKey, pageFactory);

        // RootPage
        if (String.IsNullOrEmpty(pageKey))
            RootPage = new LazyValue<Page>(() => pageFactory(this.BuildRoute(String.Empty)));
    }

    public override Task PushAsync(Page page)
    {
        if (page.Equals(CurrentPage))
            return Task.CompletedTask;

        var previousPage = CurrentPage;

        pageStack.Clear();
        pageStack.Add(page);
        CurrentPage = page;

        ContainerPage.Value.UpdatePage(CurrentPage);
        OnPageNavigated(previousPage, CurrentPage);

        return Task.CompletedTask;
    }

    #endregion
}