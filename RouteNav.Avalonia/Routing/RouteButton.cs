using System;
using Avalonia;
using Avalonia.Controls;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Routing;

/// <summary>A <see cref="Button"/> that navigates to a route when clicked.</summary>
public class RouteButton : Button, IRouteItem
{
    /// <summary>Defines the <see cref="RouteUri"/> property.</summary>
    public static readonly StyledProperty<Uri> RouteUriProperty = AvaloniaProperty.Register<RouteButton, Uri>(nameof(RouteUri));

    /// <summary>Defines the <see cref="Target"/> property.</summary>
    public static readonly StyledProperty<NavigationTarget> TargetProperty = AvaloniaProperty.Register<RouteButton, NavigationTarget>(nameof(Target), NavigationTarget.Self);

    /// <summary>Initializes a new instance of the <see cref="RouteButton"/> class.</summary>
    public RouteButton()
    {
        Target = NavigationTarget.Self;

        Click += (_, _) => NavigateToRoute();
    }

    protected override Type StyleKeyOverride => typeof(Button);

    #region Implementation of IRouteItem

    /// <summary>Gets or sets the route this button navigates to.</summary>
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

    /// <summary>Gets or sets where the target route is shown when the button is clicked.</summary>
    public NavigationTarget Target
    {
        get { return GetValue(TargetProperty); }
        set { SetValue(TargetProperty, value); }
    }

    /// <summary>Navigates to <see cref="RouteUri"/> using <see cref="Target"/>.</summary>
    public void NavigateToRoute()
    {
        Navigation.PushAsync(RouteUri, Target);
    }

    #endregion
}