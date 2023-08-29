using System;
using System.Collections.Generic;
using Avalonia.Layout;
using RouteNav.Avalonia.StackControls;

namespace RouteNav.Avalonia.Stacks;

public class NavigationPageStack : NavigationStackBase<NavigationPageContainer>, INavigationStack
{
    public NavigationPageStack(string name)
        : base(name)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
    }

    private Dictionary<string, Func<Uri, Page>> Pages { get; } = new Dictionary<string, Func<Uri, Page>>();

    #region Overrides of NavigationStackBase<NavigationPage>

    protected override Page? ResolveRoute(Uri routeUri)
    {
        return Pages.TryGetValue(this.GetRoutePath(routeUri), out var pageFactory) ? pageFactory(routeUri) : null;
    }

    protected override NavigationPageContainer InitContainer()
    {
        return new NavigationPageContainer
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            NavigationStack = this
        };
    }

    public override void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
    {
        var pageKey = relativeRoute.TrimStart('/');
        if (!Pages.ContainsKey(pageKey))
            Pages.Add(pageKey, pageFactory);
        else
            Pages[pageKey] = pageFactory;

        // RootPage
        if (String.IsNullOrEmpty(pageKey))
            RootPage = new LazyValue<Page>(() => pageFactory(this.BuildRoute(String.Empty)));
    }

    #endregion
}