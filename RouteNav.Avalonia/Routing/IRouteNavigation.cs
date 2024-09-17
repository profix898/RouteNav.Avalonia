using System;
using System.Threading.Tasks;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Routing;

/// <summary>Defines the contract for URI-based navigation.</summary>
public interface IRouteNavigation
{
    /// <summary>Occurs when a route is navigated (from Uri -> to Uri).</summary>
    public event Action<NavigationEventArgs<Uri>> RouteNavigated;

    /// <summary>
    /// Pushes a new page onto the navigation stack using a relative route.
    /// </summary>
    /// <param name="relativeRoute">The relative route to navigate to.</param>
    /// <param name="target">Navigation target (defaults to Self).</param>
    /// <returns>Task with the new page as the result.</returns>
    Task<Page> PushAsync(string relativeRoute, NavigationTarget target = NavigationTarget.Self);

    /// <summary>
    /// Pushes a new page onto the navigation stack using a route URI.
    /// </summary>
    /// <param name="routeUri">The URI to navigate to.</param>
    /// <param name="target">Navigation target (defaults to Self).</param>
    /// <returns>Task with the new page as the result.</returns>
    Task<Page> PushAsync(Uri routeUri, NavigationTarget target = NavigationTarget.Self);
}