using System;
using System.Collections.Generic;
using Avalonia.Layout;
using NSE.RouteNav.Controls;
using NSE.RouteNav.Stacks.Internal;

namespace NSE.RouteNav.Stacks;

public class NavigationPageStack : NavigationStackBase<NavigationControl>, INavigationStack
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

    protected override NavigationControl InitContainer()
    {
        return new NavigationControl
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            NavigationRouter = new PageStackNavigationRouter(this)
        };
    }

    public override void AddPage(string relativeRoute, Func<Page> pageFactory)
    {
        Pages.Set(relativeRoute.TrimStart('/'), pageFactory);
    }

    #endregion
}