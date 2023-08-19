using System;
using RouteNav.Avalonia.Routing;

namespace RouteNav.Avalonia.Stacks.TODO.Flyout;

public sealed class FlyoutMenuItem
{
    public FlyoutMenuItem()
    {
        RouteUri = null!;
        Text = null!;
    }

    public FlyoutMenuItem(Uri routeUri, string text)
    {
        RouteUri = routeUri;
        Text = text;
    }

    public FlyoutMenuItem(Uri routeUri, string text, Type pageType)
    {
        RouteUri = routeUri;
        Text = text;
        PageType = pageType;
    }

    public FlyoutMenuItem(Uri routeUri, string text, Func<Page> pageFactory)
    {
        RouteUri = routeUri;
        Text = text;
        PageFactory = pageFactory;
    }

    public FlyoutMenuItem(string routePath, string text)
    {
        RouteUri = null!; // Suppress nullability warning (property 'RelativeRoute' overrides value)
        RoutePath = routePath;
        Text = text;
    }

    public FlyoutMenuItem(string routePath, string text, Type pageType)
    {
        RouteUri = null!; // Suppress nullability warning (property 'RelativeRoute' overrides value)
        RoutePath = routePath;
        Text = text;
        PageType = pageType;
    }

    public FlyoutMenuItem(string routePath, string text, Func<Page> pageFactory)
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
        set { RouteUri = value.StartsWith("/") ? new Uri(Navigation.BaseRouteUri, value.TrimEnd('/')) : new Uri(value.TrimEnd('/'), UriKind.Relative); }
    }

    public NavigationTarget Target { get; set; } = NavigationTarget.Self;

    public string Text { get; set; }

    public object? Icon { get; set; }

    public Type? PageType { get; set; }

    public Func<Page?>? PageFactory { get; set; }

    public RouteMenuItem ToMenuItem()
    {
        return new RouteMenuItem { Header = Text, Icon = Icon, RouteUri = RouteUri, Target = Target };
    }
}