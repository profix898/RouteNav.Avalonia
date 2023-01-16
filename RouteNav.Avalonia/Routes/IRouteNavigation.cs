using System;
using System.Threading.Tasks;
using NSE.RouteNav.Stacks;

namespace NSE.RouteNav.Routes;

public interface IRouteNavigation
{
    public event Action<(Uri? routeFrom, Uri? routeTo)> RouteNavigated;

    Task<Page> PushAsync(string relativeRoute, NavigationTarget target = NavigationTarget.Self);

    Task<Page> PushAsync(Uri routeUri, NavigationTarget target = NavigationTarget.Self);
}