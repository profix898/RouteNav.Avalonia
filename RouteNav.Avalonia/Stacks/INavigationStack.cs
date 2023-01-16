using System;
using Avalonia.Controls;
using NSE.RouteNav.Dialogs;
using NSE.RouteNav.Pages;
using NSE.RouteNav.Routes;
using NSE.RouteNav.Stacks.Internal;

namespace NSE.RouteNav.Stacks;

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

    LazyValue<ContentControl> ContainerPage { get; }

    Page RootPage { get; }

    Page CurrentPage { get; }

    INavigationStack? RequestStack(string stackName);

    void AddPage(string relativeRoute, Func<Page> pageFactory);

    void AddPage(string relativeRoute, Type pageType);

    void Reset();
}