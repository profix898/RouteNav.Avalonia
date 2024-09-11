using System;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

/// <summary>
/// Specifies the preferred target for navigation (may be ignored by the navigation stack implementation).
/// </summary>
public enum NavigationTarget
{
    /// <summary>Opens the route in the current context (i.e. the same stack container).</summary>
    Self,
    /// <summary>Opens the route in the parent context (i.e. the same window).</summary>
    Parent,
    /// <summary>Opens the route in a dialog (associated with the related stack).</summary>
    Dialog,
    /// <summary>Opens the route in an overlay dialog (on top of the related stack).</summary>
    DialogOverlay,
    /// <summary>Opens the route in a new window (potentially switching to the related stack).</summary>
    Window
}

public interface INavigationStack : IPageNavigation, IDialogNavigation, IRouteNavigation
{
    string Name { get; }

    string Title { get; }

    Uri BaseUri { get; }

    bool IsMainStack { get; }

    bool IsEventStack { get; }

    public event Action Entered;

    public event Action Exited;

    LazyValue<NavigationContainer> ContainerPage { get; }

    LazyValue<Page> RootPage { get; }

    INavigationStack? RequestStack(string stackName);

    void AddPage(string relativeRoute, Func<Uri, Page> pageFactory);

    void AddPage(string relativeRoute, Type pageType);

    void Reset();
}