using System;
using System.Windows.Input;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Routing;

public class RouteCommand : ICommand, IRouteItem
{
    public RouteCommand()
    {
        Target = NavigationTarget.Self;
    }

    public RouteCommand(IRouteItem routeItem)
    {
        RouteUri = routeItem.RouteUri;
        Target = routeItem.Target;
    }

    #region Implementation of IRouteItem

    public Uri RouteUri { get; set; }

    /// <summary>Set RouteUri via route path. Both relative paths (e.g. 'myPage' relative to current stack) and
    ///          absolute paths (e.g. '/myStack/myPage') are supported. The leading '/' denotes an absolute path.</summary>
    public string RoutePath
    {
        set { RouteUri = value.StartsWith("/") ? new Uri(Navigation.BaseRouteUri, value.TrimEnd('/')) : new Uri(value.TrimEnd('/'), UriKind.Relative); }
    }

    public NavigationTarget Target { get; set; }

    public void NavigateToRoute()
    {
        Navigation.PushAsync(RouteUri, Target);
    }

    #endregion

    #region Implementation of ICommand

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        NavigateToRoute();
    }

    public event EventHandler? CanExecuteChanged;

    #endregion
}
