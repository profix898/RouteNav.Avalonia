using System;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

/// <summary>Represents a navigation stack that supports page, dialog, and route navigation.</summary>
public interface INavigationStack : IPageNavigation, IDialogNavigation, IRouteNavigation
{
    /// <summary>Gets the name of the navigation stack.</summary>
    string Name { get; }

    /// <summary>Gets the title of the navigation stack.</summary>
    string Title { get; }

    /// <summary>Gets the base URI of the navigation stack (incl. the stack's route name).</summary>
    Uri BaseUri { get; }

    /// <summary>Gets a value indicating whether this stack is the main stack for the application.</summary>
    bool IsMainStack { get; }

    /// <summary>Gets a value indicating whether this stack is an event stack.</summary>
    bool IsEventStack { get; }

    /// <summary>Occurs when the navigation stack is entered.</summary>
    event Action Entered;

    /// <summary>Occurs when the navigation stack is exited.</summary>
    event Action Exited;

    /// <summary>Gets the container page of the navigation stack.</summary>
    LazyValue<NavigationContainer> ContainerPage { get; }

    /// <summary>Gets the root page of the navigation stack.</summary>
    LazyValue<Page> RootPage { get; }

    /// <summary>Gets or sets the resolver for dynamically resolving pages based on the given route URI.</summary>
    IPageResolver? PageResolver { get; set; }

    /// <summary>Adds a page to the navigation stack using the specified relative route and page type.</summary>
    /// <param name="relativeRoute">Relative route of the page.</param>
    /// <param name="pageType">Type of the page.</param>
    void AddPage(string relativeRoute, Type pageType);

    /// <summary>Adds a page to the navigation stack using the specified relative route and page factory.</summary>
    /// <param name="relativeRoute">Relative route of the page.</param>
    /// <param name="pageFactory">Factory function for instantiating the page.</param>
    void AddPage(string relativeRoute, Func<Uri, Page> pageFactory);

    /// <summary>Checks if a stac is known to the current stack (or refers to an external stack).</summary>
    /// <param name="stackName">Name of the requested stack.</param>
    /// <returns>The requested navigation stack, or null if not found.</returns>
    INavigationStack? RequestStack(string stackName);

    /// <summary>Resets the navigation stack (upon exiting it).</summary>
    void Reset();
}