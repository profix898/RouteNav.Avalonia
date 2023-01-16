using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using NSE.RouteNav.Stacks;

namespace NSE.RouteNav.Routes;

public class RouteMenuItem : MenuItem, IStyleable
{
    private Uri routeUri;

    private NavigationTarget target;

    public static readonly DirectProperty<RouteMenuItem, Uri> RouteUriProperty = AvaloniaProperty.RegisterDirect<RouteMenuItem, Uri>(nameof(RouteUri), rmi => rmi.routeUri, (rmi, value) => rmi.routeUri = value);

    public static readonly DirectProperty<RouteMenuItem, NavigationTarget> TargetProperty = AvaloniaProperty.RegisterDirect<RouteMenuItem, NavigationTarget>(nameof(Target), rmi => rmi.target, (rmi, value) => rmi.target = value);

    public RouteMenuItem()
    {
        Target = NavigationTarget.Self;

        Click += (_, _) => NavigateToRoute();
    }

    Type IStyleable.StyleKey => typeof(MenuItem);

    public Uri RouteUri
    {
        get { return routeUri; }
        set { SetAndRaise(RouteUriProperty, ref routeUri, value); }
    }

    /// <summary>Set RouteUri via route path. Both relative paths (e.g. 'myPage' relative to current stack) and
    ///          absolute paths (e.g. '/myStack/myPage') are supported. The leading '/' denotes an absolute path.</summary>
    public string RoutePath
    {
        set { SetAndRaise(RouteUriProperty, ref routeUri, value.StartsWith("/") ? new Uri(Navigation.BaseRouteUri, value + "/") : new Uri(value, UriKind.Relative)); }
    }

    public NavigationTarget Target
    {
        get { return target; }
        set { SetAndRaise(TargetProperty, ref target, value); }
    }

    public virtual void NavigateToRoute()
    {
        Navigation.PushAsync(RouteUri, Target);
    }
}