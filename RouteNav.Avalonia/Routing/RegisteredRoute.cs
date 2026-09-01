using System;

namespace RouteNav.Avalonia.Routing;

/// <summary>A route (with its registered page source) as exposed by <see cref="Stacks.INavigationStack.RegisteredRoutes" />.</summary>
public readonly struct RegisteredRoute
{
    /// <summary>Initializes a new instance of the <see cref="RegisteredRoute" /> struct.</summary>
    public RegisteredRoute(string routePath, Type? pageType, Func<Uri, Page>? pageFactory)
    {
        RoutePath = routePath;
        PageType = pageType;
        PageFactory = pageFactory;
    }

    /// <summary>Gets the stack-relative route path (the key used in <see cref="Stacks.INavigationStack.AddPage" />).</summary>
    public string RoutePath { get; }

    /// <summary>Gets the page type registered for this route, or <c>null</c> when the route was registered via a factory.</summary>
    public Type? PageType { get; }

    /// <summary>Gets the page factory registered for this route, or <c>null</c> when only a page type is known.</summary>
    public Func<Uri, Page>? PageFactory { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return RoutePath;
    }
}
