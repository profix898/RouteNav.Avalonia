using System;
using System.Threading.Tasks;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Routing;

public interface IRouteNavigation
{
    public event Action<(Uri? routeFrom, Uri? routeTo)> RouteNavigated;

    Task<Page> PushAsync(string relativeRoute, NavigationTarget target = NavigationTarget.Self);

    Task<Page> PushAsync(Uri routeUri, NavigationTarget target = NavigationTarget.Self);
}