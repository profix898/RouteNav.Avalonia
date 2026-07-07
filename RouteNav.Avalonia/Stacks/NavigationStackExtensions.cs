using System;
using System.Collections.Generic;
using System.Linq;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Routing;

namespace RouteNav.Avalonia.Stacks;

/// <summary>High-level extension helpers for building routes and registering pages/menu items on navigation stacks.</summary>
public static class NavigationStackExtensions
{
    #region Route

    /// <summary>Builds an absolute route URI on the stack from a relative route string.</summary>
    public static Uri BuildRoute(this INavigationStack stack, string relativeRoute)
    {
        return new Uri(new Uri(stack.BaseUri.AbsoluteUri + "/"), relativeRoute);
    }

    /// <summary>Builds an absolute route URI on the stack from a relative route URI.</summary>
    public static Uri BuildRoute(this INavigationStack stack, Uri relativeRoute)
    {
        if (relativeRoute.IsAbsoluteUri)
            return relativeRoute;

        return new Uri(new Uri(stack.BaseUri.AbsoluteUri + "/"), relativeRoute);
    }

    /// <summary>Gets the stack-relative route path for the given route URI.</summary>
    public static string GetRoutePath(this INavigationStack stack, Uri routeUri)
    {
        return stack.GetRoutePath(routeUri, out _);
    }

    /// <summary>Gets the stack-relative route path and query for the given route URI.</summary>
    public static string GetRoutePath(this INavigationStack stack, Uri routeUri, out string query)
    {
        query = String.Empty;

        // AbsoluteUri -> RelativeUri
        if (routeUri.IsAbsoluteUri)
        {
            if (!routeUri.AbsoluteUri.StartsWith(stack.BaseUri.AbsoluteUri))
                return routeUri.AbsolutePath; // Absolute Uri does not resolve to given navigation stack

            routeUri = new Uri(routeUri.AbsoluteUri.Substring(stack.BaseUri.AbsoluteUri.Length).Trim('/'), UriKind.Relative);
        }

        // Empty Uri path/query
        if (String.IsNullOrEmpty(routeUri.OriginalString))
        {
            query = String.Empty;
            return String.Empty;
        }

        // Empty Uri path (only query part)
        if (routeUri.OriginalString.StartsWith('?') || routeUri.OriginalString.StartsWith('#'))
        {
            query = routeUri.OriginalString;
            return String.Empty;
        }

        // Uri path + query
        var idx = routeUri.OriginalString.IndexOf('?');
        var path = idx != -1 ? routeUri.OriginalString.Substring(0, idx) : routeUri.OriginalString;
        query = idx != -1 ? routeUri.OriginalString.Substring(idx) : String.Empty;

        return path;
    }

    /// <summary>Gets a value indicating whether two route URIs resolve to the same route path on the stack.</summary>
    public static bool EqualsRoutePath(this INavigationStack stack, Uri routeUriA, Uri routeUriB)
    {
        return stack.GetRoutePath(routeUriA).Equals(stack.GetRoutePath(routeUriB));
    }

    /// <summary>Gets the stack name from an absolute route URI, or <c>null</c> for a relative URI.</summary>
    public static string? GetStackName(this Uri routeUri)
    {
        if (!routeUri.IsAbsoluteUri)
            return null;

        // Is absolute route (with stackName), if it contains at least two segments, i.e. '/stackName/path'
        var segments = routeUri.Segments;
        if (segments[0] == "/" && segments.Length > 1)
            return segments[1].Trim('/');

        return segments[0].Trim('/');
    }

    /// <summary>Gets a value indicating whether the route URI targets the given stack (relative URIs assume the same stack).</summary>
    public static bool IsRouteOnStack(this Uri routeUri, INavigationStack stack)
    {
        var stackName = routeUri.GetStackName();
        if (stackName == null)
            return true; // Relative route -> assume same stack

        return stackName.Equals(stack.Name, StringComparison.InvariantCulture);
    }

    #endregion

    #region GetStack

    /// <summary>Gets the main stack from the sequence.</summary>
    public static INavigationStack GetMainStack(this IEnumerable<INavigationStack> navStacks)
    {
        return navStacks.GetStack(Navigation.MainStackName) ?? throw new NavigationException($"Stack '{Navigation.MainStackName}' is not available.");
    }

    /// <summary>Gets the stack with the given name from the sequence, or <c>null</c> if not found.</summary>
    public static INavigationStack? GetStack(this IEnumerable<INavigationStack> navStacks, string stackName)
    {
        return navStacks.FirstOrDefault(stack => stack.Name.Equals(stackName));
    }

    #endregion

    #region AddPage

    /// <summary>Registers a page type at the given relative route.</summary>
    public static void AddPage<TPage>(this INavigationStack stack, string relativeRoute)
        where TPage : Page
    {
        stack.AddPage(relativeRoute, typeof(TPage));
    }

    /// <summary>Registers multiple pages from a route-to-type map.</summary>
    public static void AddPage(this INavigationStack stack, IDictionary<string, Type> pageMap)
    {
        foreach (var (relativeRoute, pageType) in pageMap)
            stack.AddPage(relativeRoute, pageType);
    }

    #endregion

    #region AddPage_RouteButton

    /// <summary>Registers a page type at the route carried by the given <see cref="RouteButton" />.</summary>
    public static void AddPage<T1>(this INavigationStack stack, RouteButton routeButton)
        where T1 : Page
    {
        if (!routeButton.RouteUri.IsAbsoluteUri)
            routeButton.RouteUri = stack.BuildRoute(routeButton.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeButton.RouteUri)
                      ?? throw new ArgumentException("Invalid route URI.", nameof(routeButton.RouteUri)), typeof(T1));
    }

    /// <summary>Registers a page type at the route carried by the given <see cref="RouteButton" />.</summary>
    public static void AddPage(this INavigationStack stack, RouteButton routeButton, Type pageType)
    {
        if (!routeButton.RouteUri.IsAbsoluteUri)
            routeButton.RouteUri = stack.BuildRoute(routeButton.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeButton.RouteUri)
                      ?? throw new ArgumentException("Invalid route URI.", nameof(routeButton.RouteUri)), pageType);
    }

    /// <summary>Registers a page factory at the route carried by the given <see cref="RouteButton" />.</summary>
    public static void AddPage(this INavigationStack stack, RouteButton routeButton, Func<Uri, Page> pageFactory)
    {
        if (!routeButton.RouteUri.IsAbsoluteUri)
            routeButton.RouteUri = stack.BuildRoute(routeButton.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeButton.RouteUri)
                      ?? throw new ArgumentException("Invalid route URI.", nameof(routeButton.RouteUri)), pageFactory);
    }

    #endregion

    #region AddPage_RouteMenuItem

    /// <summary>Registers a page type at the route carried by the given <see cref="RouteMenuItem" />.</summary>
    public static void AddPage<T1>(this INavigationStack stack, RouteMenuItem routeMenuItem)
        where T1 : Page
    {
        if (!routeMenuItem.RouteUri.IsAbsoluteUri)
            routeMenuItem.RouteUri = stack.BuildRoute(routeMenuItem.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeMenuItem.RouteUri)
                      ?? throw new ArgumentException("Invalid route URI.", nameof(routeMenuItem.RouteUri)), typeof(T1));
    }

    /// <summary>Registers a page type at the route carried by the given <see cref="RouteMenuItem" />.</summary>
    public static void AddPage(this INavigationStack stack, RouteMenuItem routeMenuItem, Type pageType)
    {
        if (!routeMenuItem.RouteUri.IsAbsoluteUri)
            routeMenuItem.RouteUri = stack.BuildRoute(routeMenuItem.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeMenuItem.RouteUri)
                      ?? throw new ArgumentException("Invalid route URI.", nameof(routeMenuItem.RouteUri)), pageType);
    }

    /// <summary>Registers a page factory at the route carried by the given <see cref="RouteMenuItem" />.</summary>
    public static void AddPage(this INavigationStack stack, RouteMenuItem routeMenuItem, Func<Uri, Page> pageFactory)
    {
        if (!routeMenuItem.RouteUri.IsAbsoluteUri)
            routeMenuItem.RouteUri = stack.BuildRoute(routeMenuItem.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeMenuItem.RouteUri)
                      ?? throw new ArgumentException("Invalid route URI.", nameof(routeMenuItem.RouteUri)), pageFactory);
    }

    #endregion

    #region AddPage_SidebarMenuItem

    /// <summary>Adds a sidebar menu item for the given relative route.</summary>
    public static void AddMenuItem(this ISidebarMenuPageStack stack, string relativeRoute, string text, NavigationTarget target = NavigationTarget.Self)
    {
        var menuItem = new SidebarMenuItem { RoutePath = relativeRoute, Text = text, Target = target };
        stack.AddMenuItem(menuItem);
    }

    /// <summary>Adds a sidebar menu item for the given relative route and registers the page type when it targets this stack.</summary>
    public static void AddMenuItem<TPage>(this ISidebarMenuPageStack stack, string relativeRoute, string text, NavigationTarget target = NavigationTarget.Self)
    {
        var menuItem = new SidebarMenuItem { RoutePath = relativeRoute, Text = text, Target = target };
        stack.AddMenuItem(menuItem);

        if (menuItem.RouteUri.IsRouteOnStack(stack))
            stack.AddPage(relativeRoute, typeof(TPage));
    }

    /// <summary>Adds a sidebar menu item for the given relative route and registers the page type when it targets this stack.</summary>
    public static void AddMenuItem(this ISidebarMenuPageStack stack, string relativeRoute, string text, Type pageType, NavigationTarget target = NavigationTarget.Self)
    {
        var menuItem = new SidebarMenuItem { RoutePath = relativeRoute, Text = text, Target = target };
        stack.AddMenuItem(menuItem);

        if (menuItem.RouteUri.IsRouteOnStack(stack))
            stack.AddPage(relativeRoute, pageType);
    }

    /// <summary>Adds a sidebar menu item for the given relative route and registers the page factory when it targets this stack.</summary>
    public static void AddMenuItem(this ISidebarMenuPageStack stack, string relativeRoute, string text, Func<Uri, Page> pageFactory,
                                   NavigationTarget target = NavigationTarget.Self)
    {
        var menuItem = new SidebarMenuItem { RoutePath = relativeRoute, Text = text, Target = target };
        stack.AddMenuItem(menuItem);

        if (menuItem.RouteUri.IsRouteOnStack(stack))
            stack.AddPage(relativeRoute, pageFactory);
    }

    /// <summary>Adds a sidebar menu item for the given route URI.</summary>
    public static void AddMenuItem(this ISidebarMenuPageStack stack, Uri routeUri, string text, NavigationTarget target = NavigationTarget.Self)
    {
        var menuItem = new SidebarMenuItem { RouteUri = routeUri, Text = text, Target = target };
        stack.AddMenuItem(menuItem);
    }

    /// <summary>Adds a sidebar menu item for the given route URI and registers the page type when it targets this stack.</summary>
    public static void AddMenuItem<TPage>(this ISidebarMenuPageStack stack, Uri routeUri, string text, NavigationTarget target = NavigationTarget.Self)
    {
        var menuItem = new SidebarMenuItem { RouteUri = routeUri, Text = text, Target = target };
        stack.AddMenuItem(menuItem);

        if (menuItem.RouteUri.IsRouteOnStack(stack))
            stack.AddPage(stack.GetRoutePath(menuItem.RouteUri), typeof(TPage));
    }

    /// <summary>Adds a sidebar menu item for the given route URI and registers the page type when it targets this stack.</summary>
    public static void AddMenuItem(this ISidebarMenuPageStack stack, Uri routeUri, string text, Type pageType, NavigationTarget target = NavigationTarget.Self)
    {
        var menuItem = new SidebarMenuItem { RouteUri = routeUri, Text = text, Target = target };
        stack.AddMenuItem(menuItem);

        if (menuItem.RouteUri.IsRouteOnStack(stack))
            stack.AddPage(stack.GetRoutePath(menuItem.RouteUri), pageType);
    }

    /// <summary>Adds a sidebar menu item for the given route URI and registers the page factory when it targets this stack.</summary>
    public static void AddMenuItem(this ISidebarMenuPageStack stack, Uri routeUri, string text, Func<Uri, Page> pageFactory, NavigationTarget target = NavigationTarget.Self)
    {
        var menuItem = new SidebarMenuItem { RouteUri = routeUri, Text = text, Target = target };
        stack.AddMenuItem(menuItem);

        if (menuItem.RouteUri.IsRouteOnStack(stack))
            stack.AddPage(stack.GetRoutePath(menuItem.RouteUri), pageFactory);
    }

    #endregion
}
