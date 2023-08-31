using System;
using RouteNav.Avalonia.Routing;

namespace RouteNav.Avalonia.Stacks;

public sealed class SidebarMenuItem
{
    public SidebarMenuItem()
    {
        RouteUri = null!;
        Text = null!;
    }

    public SidebarMenuItem(Uri routeUri, string text)
    {
        RouteUri = routeUri;
        Text = text;
    }

    public SidebarMenuItem(Uri routeUri, string text, Type pageType)
    {
        RouteUri = routeUri;
        Text = text;
        PageType = pageType;
    }

    public SidebarMenuItem(Uri routeUri, string text, Func<Uri, Page> pageFactory)
    {
        RouteUri = routeUri;
        Text = text;
        PageFactory = pageFactory;
    }

    public SidebarMenuItem(string routePath, string text)
    {
        RouteUri = null!; // Suppress nullability warning (property 'RelativeRoute' overrides value)
        RoutePath = routePath;
        Text = text;
    }

    public SidebarMenuItem(string routePath, string text, Type pageType)
    {
        RouteUri = null!; // Suppress nullability warning (property 'RelativeRoute' overrides value)
        RoutePath = routePath;
        Text = text;
        PageType = pageType;
    }

    public SidebarMenuItem(string routePath, string text, Func<Uri, Page> pageFactory)
    {
        RouteUri = null!; // Suppress nullability warning (property 'RelativeRoute' overrides value)
        RoutePath = routePath;
        Text = text;
        PageFactory = pageFactory;
    }

    public Uri RouteUri { get; set; }

    /// <summary>Set RouteUri via route path. Both relative paths (e.g. 'myPage' relative to current stack) and
    ///          absolute paths (e.g. '/myStack/myPage') are supported. The leading '/' denotes an absolute path.</summary>
    public string RoutePath
    {
        set { RouteUri = value.StartsWith("/") ? new Uri(Navigation.BaseRouteUri, value + "/") : new Uri(value, UriKind.Relative); }
    }

    public NavigationTarget Target { get; set; } = NavigationTarget.Self;

    public string Text { get; set; }

    public object? Icon { get; set; }

    public Type? PageType { get; set; }

    public Func<Uri, Page>? PageFactory { get; set; }

    public RouteMenuItem ToMenuItem()
    {
        return new RouteMenuItem { Header = Text, Icon = Icon, RouteUri = RouteUri, Target = Target };
    }
}