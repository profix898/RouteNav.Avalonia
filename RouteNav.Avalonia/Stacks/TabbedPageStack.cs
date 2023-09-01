using System;
using System.Collections.Generic;
using Avalonia.Controls;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Platform;
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

    internal void SetCurrentPage(Page? page)
    {
        CurrentPage = page;
    }

    #region Overrides of NavigationStackBase<TabbedPage>

    protected override Page? ResolveRoute(Uri routeUri)
    {
        return Pages.TryGetValue(this.GetRoutePath(routeUri), out var pageFactory) ? pageFactory(routeUri) : null;
    }

    protected override TC InitContainer()
    {
        var tabbedPageContainer = new TC { NavigationStack = this };
        tabbedPageContainer.HostControlAttached += () =>
        {
            if (tabbedPageContainer.TabControl == null)
                throw new InvalidOperationException($"No {nameof(TabControl)} found in NavigationContainer.");

            var items = new List<TabItem>();
            foreach (var pageKvp in Pages)
            {
                var page = pageKvp.Value(this.BuildRoute(pageKvp.Key));
                var tabItem = new TabItem { Header = page.Title, Content = page };
                items.Add(tabItem);

                if (pageKvp.Key == String.Empty) // Initial page
                    rootPage = page;
            }
            rootPage ??= tabbedPageContainer.TabControl.SelectedItem as Page ?? new NotFoundPage();
            RootPage = new LazyValue<Page>(() => rootPage);

            tabbedPageContainer.TabControl.ItemsSource = items;
            tabbedPageContainer.TabControl.SelectedItem = TabbedPageContainer.FindTabItem(tabbedPageContainer.TabControl, rootPage);
        };

        return tabbedPageContainer;
    }

    public override void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
    {
        Pages.Set(relativeRoute.TrimStart('/'), pageFactory);
    }

    #endregion
}