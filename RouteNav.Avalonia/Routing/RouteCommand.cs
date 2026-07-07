using System;
using System.Windows.Input;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Routing;

/// <summary>An <see cref="ICommand"/> that navigates to a route when executed.</summary>
public class RouteCommand : ICommand, IRouteItem
{
    /// <summary>Initializes a new instance of the <see cref="RouteCommand"/> class.</summary>
    public RouteCommand()
    {
        Target = NavigationTarget.Self;
    }

    /// <summary>Initializes a new instance of the <see cref="RouteCommand"/> class from an existing route item.</summary>
    public RouteCommand(IRouteItem routeItem)
    {
        RouteUri = routeItem.RouteUri;
        Target = routeItem.Target;
    }

    #region Implementation of IRouteItem

    /// <summary>Gets or sets the route this command navigates to.</summary>
    public Uri RouteUri { get; set; }

    /// <summary>Set RouteUri via route path. Both relative paths (e.g. 'myPage' relative to current stack) and
    ///          absolute paths (e.g. '/myStack/myPage') are supported. The leading '/' denotes an absolute path.</summary>
    public string RoutePath
    {
        set { RouteUri = value.StartsWith("/") ? new Uri(Navigation.BaseRouteUri, value.TrimEnd('/')) : new Uri(value.TrimEnd('/'), UriKind.Relative); }
    }

    /// <summary>Gets or sets where the target route is shown when the command executes.</summary>
    public NavigationTarget Target { get; set; }

    /// <summary>Navigates to <see cref="RouteUri"/> using <see cref="Target"/>.</summary>
    public void NavigateToRoute()
    {
        Navigation.PushAsync(RouteUri, Target);
    }

    #endregion

    #region Implementation of ICommand

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return true;
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        NavigateToRoute();
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    #endregion
}
