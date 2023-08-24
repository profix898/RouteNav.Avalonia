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

    private Dictionary<string, Func<Page>> Pages { get; } = new Dictionary<string, Func<Page>>();

    #region Overrides of NavigationStackBase<NavigationPage>

    protected override Page? ResolveRoute(Uri routeUri)
    {
        return Pages.TryGetValue(this.GetRoutePath(routeUri), out var pageFactory) ? pageFactory() : null;
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

    public override void AddPage(string relativeRoute, Func<Page> pageFactory)
    {
        var key = relativeRoute.TrimStart('/');
        if (!Pages.ContainsKey(key))
            Pages.Add(key, pageFactory);
        else
            Pages[key] = pageFactory;
    }

    #endregion
}