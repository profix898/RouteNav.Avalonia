using System;

namespace RouteNav.Avalonia.Pages;

/// <summary>
/// Defines a mechanism for dynamically resolving pages based on a given route URI (without prior explicit registration).
/// </summary>
public interface IPageResolver
{
    /// <summary>
    /// Resolves a page based on the provided route URI.
    /// </summary>
    /// <param name="routeUri">URI of the route to resolve.</param>
    /// <returns>
    /// The resolved <see cref="Page"/> if the route is successfully resolved; otherwise <c>null</c>.
    /// </returns>
    Page? ResolveRoute(Uri routeUri);
}
