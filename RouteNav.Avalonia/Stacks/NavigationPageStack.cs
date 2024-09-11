using System;
using System.Collections.Generic;
using Avalonia.Layout;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

public class NavigationPageStack : NavigationPageStack<NavigationPageContainer>
{
    public NavigationPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }
}

public class NavigationPageStack<TC> : NavigationStackBase<TC>, INavigationStack
    where TC : NavigationPageContainer, new()
{
    private readonly Dictionary<string, Func<Uri, Page>> pages = new Dictionary<string, Func<Uri, Page>>();

    public NavigationPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }

    #region Overrides of NavigationStackBase<NavigationPage>

    protected override Page? ResolveRoute(Uri routeUri)
    {
        return pages.TryGetValue(this.GetRoutePath(routeUri), out var pageFactory) ? pageFactory(routeUri) : null;
    }

    protected override TC InitContainer()
    {
        var navigationPageContainer = new TC
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            NavigationStack = this
        };
        navigationPageContainer.HostControlAttached += () =>
        {
            if (navigationPageContainer.NavigationControl == null)
                throw new InvalidOperationException($"No {nameof(NavigationControl)} found in NavigationContainer.");
        };

        return navigationPageContainer;
    }

    public override void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
    {
        var pageKey = relativeRoute.Trim('/');
        pages.Set(pageKey, pageFactory);

        // RootPage
        if (String.IsNullOrEmpty(pageKey))
            RootPage = new LazyValue<Page>(() => pageFactory(this.BuildRoute(String.Empty)));
    }

    #endregion
}