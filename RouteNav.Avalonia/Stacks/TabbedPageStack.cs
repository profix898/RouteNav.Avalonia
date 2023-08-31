using System;
using System.Collections.Generic;
using Avalonia.Controls;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

public class TabbedPageStack : TabbedPageStack<TabbedPageContainer>
{
    public TabbedPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }
}

public class TabbedPageStack<TC> : NavigationStackBase<TC>, IPageNavigation, IRouteNavigation, INavigationStack
    where TC : TabbedPageContainer, new()

{
    private Page? rootPage;

    public TabbedPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }

    private Dictionary<string, Func<Uri, Page>> Pages { get; } = new Dictionary<string, Func<Uri, Page>>();

    #region Overrides of NavigationStackBase<TabbedPage>

    protected override Page? ResolveRoute(Uri routeUri)
    {
        return Pages.TryGetValue(this.GetRoutePath(routeUri), out var pageFactory) ? pageFactory(routeUri) : null;
    }

    protected override TC InitContainer()
    {
        var tabbedPageContainer = new TC { NavigationStack = this };
        if (tabbedPageContainer.TabControl == null)
            throw new InvalidOperationException($"No {nameof(TabControl)} found in NavigationContainer.");

        foreach (var pageKvp in Pages)
        {
            var page = pageKvp.Value(new Uri(pageKvp.Key, UriKind.Relative));
            var tabItem = new TabItem { Header = page.Title, Content = page };
            tabbedPageContainer.TabControl.Items.Add(tabItem);

            if (pageKvp.Key == String.Empty) // Initial page
            {
                rootPage = page;
                tabbedPageContainer.TabControl.SelectedItem = tabItem;
            }
        }
        rootPage ??= (tabbedPageContainer.TabControl.SelectedItem as Page) ?? new NotFoundPage();

        return tabbedPageContainer;
    }

    public override void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
    {
        Pages.Set(relativeRoute.TrimStart('/'), pageFactory);
    }

    #endregion
}