using System;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.StackControls;

namespace RouteNav.Avalonia.Stacks;

public enum NavigationTarget
{
    Self,
    Parent,
    Dialog,
    Window
}

public interface INavigationStack : IPageNavigation, IDialogNavigation, IRouteNavigation
{
    string Name { get; }

    Uri BaseUri { get; }

    bool IsMainStack { get; }

    bool IsEventStack { get; }

    public event Action Entered;

    public event Action Exited;

    LazyValue<NavigationContainer> ContainerPage { get; }

    INavigationStack? RequestStack(string stackName);

    void AddPage(string relativeRoute, Func<Page> pageFactory);

    void AddPage(string relativeRoute, Type pageType);

    void Reset();
}