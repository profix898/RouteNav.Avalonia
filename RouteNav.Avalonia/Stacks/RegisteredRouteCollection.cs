using System;
using System.Collections;
using System.Collections.Generic;
using RouteNav.Avalonia.Routing;

namespace RouteNav.Avalonia.Stacks;

/// <summary>
/// Read-only live view over the routes registered on a navigation stack (see <see cref="INavigationStack.RegisteredRoutes" />).
/// </summary>
internal sealed class RegisteredRouteCollection : IReadOnlyList<RegisteredRoute>
{
    private readonly Dictionary<string, RegisteredRoute> routes;

    /// <summary>Initializes a new instance of the <see cref="RegisteredRouteCollection" /> class.</summary>
    internal RegisteredRouteCollection(Dictionary<string, RegisteredRoute> routes)
    {
        this.routes = routes;
    }

    /// <inheritdoc />
    public int Count => routes.Count;

    /// <inheritdoc />
    public RegisteredRoute this[int index]
    {
        get
        {
            if (index < 0 || index >= routes.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var i = 0;
            foreach (var route in routes.Values)
            {
                if (i++ == index)
                    return route;
            }

            throw new InvalidOperationException();
        }
    }

    /// <inheritdoc />
    public IEnumerator<RegisteredRoute> GetEnumerator()
    {
        return routes.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
