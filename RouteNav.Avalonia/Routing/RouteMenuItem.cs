using System;
using Avalonia;
using Avalonia.Controls;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Routing;

public class RouteMenuItem : MenuItem
{
    public static readonly StyledProperty<Uri> RouteUriProperty = AvaloniaProperty.Register<SidebarMenuItem, Uri>(nameof(RouteUri));

    public static readonly StyledProperty<NavigationTarget> TargetProperty = AvaloniaProperty.Register<SidebarMenuItem, NavigationTarget>(nameof(Target), NavigationTarget.Self);

    public RouteMenuItem()
    {
        Click += (_, _) => Navigation.PushAsync(RouteUri, Target);
    }

    protected override Type StyleKeyOverride => typeof(MenuItem);

    public Uri RouteUri
    {
        get { return GetValue(RouteUriProperty); }
        set { SetValue(RouteUriProperty, value); }
    }

    /// <summary>Set RouteUri via route path. Both relative paths (e.g. 'myPage' relative to current stack) and
    ///          absolute paths (e.g. '/myStack/myPage') are supported. The leading '/' denotes an absolute path.</summary>
    public string RoutePath
    {
        set { SetValue(RouteUriProperty, value.StartsWith("/") ? new Uri(Navigation.BaseRouteUri, value.TrimEnd('/')) : new Uri(value.TrimEnd('/'), UriKind.Relative)); }
    }

    public NavigationTarget Target
    {
        get { return GetValue(TargetProperty); }
        set { SetValue(TargetProperty, value); }
    }
}