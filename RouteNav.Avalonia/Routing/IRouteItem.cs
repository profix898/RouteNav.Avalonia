using System;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Routing;

/// <summary>Represents an item that can be navigated to within a routing system.</summary>
public interface IRouteItem
{
    /// <summary>Sets the RouteUri via route path. Both relative paths (e.g. 'myPage' relative to current stack) and
    /// absolute paths (e.g. '/myStack/myPage') are supported. The leading '/' denotes an absolute path.</summary>
    string RoutePath { set; }

    /// <summary>Gets or sets the route URI.</summary>
    Uri RouteUri { get; set; }

    /// <summary>Gets or sets the target for navigation (defaults to Self).</summary>
    NavigationTarget Target { get; set; }

    /// <summary>Invokes navigation action to the specified route.</summary>
    void NavigateToRoute();
}
