using System;
using System.Collections.Generic;
using Avalonia.Controls;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

/// <summary>A navigation stack that presents each registered page as a tab.</summary>
public class TabbedPageStack : TabbedPageStack<TabbedPageContainer>
{
    /// <summary>Initializes a new instance of the <see cref="TabbedPageStack" /> class.</summary>
    public TabbedPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }
}

/// <summary>A navigation stack that presents each registered page as a tab.</summary>
/// <typeparam name="TC">The navigation container type.</typeparam>
public class TabbedPageStack<TC> : NavigationStackBase<TC>, IPageNavigation, IRouteNavigation, INavigationStack
    where TC : TabbedPageContainer, new()

{
    private Page? rootPage;

    /// <summary>Initializes a new instance of the <see cref="TabbedPageStack{TC}" /> class.</summary>
    public TabbedPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }

    internal void SetCurrentPage(Page? page)
    {
        CurrentPage = page;
    }

    #region Overrides of NavigationStackBase<TabbedPage>

    /// <inheritdoc />
    protected override TC InitContainer()
    {
        var tabbedPageContainer = new TC { NavigationStack = this };
        var tabsInitialized = false;

        void EnsureTabsInitialized()
        {
            if (tabbedPageContainer.TabControl == null)
                throw new InvalidOperationException($"No {nameof(TabControl)} found in NavigationContainer.");
            if (tabsInitialized)
                return;

            var items = new List<TabItem>();
            foreach (var pageKvp in pages)
            {
                var page = pageKvp.Value.PageFactory!(this.BuildRoute(pageKvp.Key));
                page.RouteUri ??= this.BuildRoute(pageKvp.Key); // Enable route-path tab matching for factory-created tabs
                var tabItem = new TabItem { Header = page.Title, Content = page };
                tabItem.Classes.Add("RouteNavTabbedPageTab");
                items.Add(tabItem);

                if (pageKvp.Key == String.Empty) // Initial page
                    rootPage = page;
            }
            rootPage ??= tabbedPageContainer.TabControl.SelectedItem as Page ?? new NotFoundPage();
            RootPage = new LazyValue<Page>(() => rootPage);

            tabbedPageContainer.TabControl.ItemsSource = items;
            tabbedPageContainer.TabControl.SelectedItem = TabbedPageContainer.FindTabItem(tabbedPageContainer.TabControl, CurrentPage ?? rootPage, this);
            tabsInitialized = true;
        }

        tabbedPageContainer.HostControlAttached += EnsureTabsInitialized;
        if (tabbedPageContainer.TabControl != null)
            EnsureTabsInitialized();

        return tabbedPageContainer;
    }

    /// <inheritdoc />
    public override void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
    {
        var pageKey = relativeRoute.Trim('/');
        pages.Set(pageKey, new RegisteredRoute(pageKey, null, pageFactory));
    }

    #endregion
}
