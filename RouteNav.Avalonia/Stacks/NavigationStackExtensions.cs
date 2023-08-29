﻿using System;
using System.Collections.Generic;
using System.Linq;
using RouteNav.Avalonia.Routing;

namespace RouteNav.Avalonia.Stacks;

public static class NavigationStackExtensions
{
    #region Route

    public static Uri BuildRoute(this INavigationStack stack, string relativeRoute)
    {
        return new Uri(new Uri(stack.BaseUri.AbsoluteUri + "/"), relativeRoute);
    }

    public static Uri BuildRoute(this INavigationStack stack, Uri relativeRoute)
    {
        return new Uri(new Uri(stack.BaseUri.AbsoluteUri + "/"), relativeRoute);
    }

    public static string GetRoutePath(this INavigationStack stack, Uri routeUri)
    {
        return stack.GetRoutePath(routeUri, out _);
    }

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

    #endregion

    #region GetStack

    public static INavigationStack GetMainStack(this IEnumerable<INavigationStack> navStacks)
    {
        return navStacks.GetStack(Navigation.MainStackName) ?? throw new NavigationException($"Stack '{Navigation.MainStackName}' is not available.");
    }

    public static INavigationStack? GetStack(this IEnumerable<INavigationStack> navStacks, string stackName)
    {
        return navStacks.FirstOrDefault(stack => stack.Name.Equals(stackName));
    }

    #endregion

    #region RegisterRoute

    public static void Add(this INavigationStack stack, RouteMenuItem routeMenuItem, Func<Uri, Page> pageFactory)
    {
        if (!routeMenuItem.RouteUri.IsAbsoluteUri)
            routeMenuItem.RouteUri = stack.BuildRoute(routeMenuItem.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeMenuItem.RouteUri) ?? throw new ArgumentException("Invalid route URI.", nameof(routeMenuItem.RouteUri)), pageFactory);
    }

    public static void Add(this INavigationStack stack, RouteMenuItem routeMenuItem, Type pageType)
    {
        if (!routeMenuItem.RouteUri.IsAbsoluteUri)
            routeMenuItem.RouteUri = stack.BuildRoute(routeMenuItem.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeMenuItem.RouteUri) ?? throw new ArgumentException("Invalid route URI.", nameof(routeMenuItem.RouteUri)), pageType);
    }

    public static void Add<T1>(this INavigationStack stack, RouteMenuItem routeMenuItem)
        where T1 : Page
    {
        if (!routeMenuItem.RouteUri.IsAbsoluteUri)
            routeMenuItem.RouteUri = stack.BuildRoute(routeMenuItem.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeMenuItem.RouteUri) ?? throw new ArgumentException("Invalid route URI.", nameof(routeMenuItem.RouteUri)), typeof(T1));
    }

    public static void Add(this INavigationStack stack, RouteButton routeButton, Func<Uri, Page> pageFactory)
    {
        if (!routeButton.RouteUri.IsAbsoluteUri)
            routeButton.RouteUri = stack.BuildRoute(routeButton.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeButton.RouteUri) ?? throw new ArgumentException("Invalid route URI.", nameof(routeButton.RouteUri)), pageFactory);
    }

    public static void Add(this INavigationStack stack, RouteButton routeButton, Type pageType)
    {
        if (!routeButton.RouteUri.IsAbsoluteUri)
            routeButton.RouteUri = stack.BuildRoute(routeButton.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeButton.RouteUri) ?? throw new ArgumentException("Invalid route URI.", nameof(routeButton.RouteUri)), pageType);
    }

    public static void Add<T1>(this INavigationStack stack, RouteButton routeButton)
        where T1 : Page
    {
        if (!routeButton.RouteUri.IsAbsoluteUri)
            routeButton.RouteUri = stack.BuildRoute(routeButton.RouteUri); // Change route to include stackName

        stack.AddPage(stack.GetRoutePath(routeButton.RouteUri) ?? throw new ArgumentException("Invalid route URI.", nameof(routeButton.RouteUri)), typeof(T1));
    }

    #endregion
}