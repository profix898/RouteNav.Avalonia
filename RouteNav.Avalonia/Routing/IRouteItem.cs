using System;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Routing;

public interface IRouteItem
{
    string RoutePath { set; }

    Uri RouteUri { get; set; }

    NavigationTarget Target { get; set; }

    void NavigateToRoute();
}
