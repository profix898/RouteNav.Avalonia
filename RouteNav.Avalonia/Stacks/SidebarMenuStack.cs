using System;
using System.Collections.Generic;
using System.Linq;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.StackContainers;
using static RouteNav.Avalonia.Controls.SidebarMenu;

namespace RouteNav.Avalonia.Stacks;

public class SidebarMenuStack : SidebarMenuStack<SidebarMenuContainer>
{
    public SidebarMenuStack(string name, string title)
        : base(name, title)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }
}

public class SidebarMenuStack<TC> : NavigationStackBase<TC>, INavigationStack
    where TC : SidebarMenuContainer, new()
{
    private readonly List<SidebarMenuItem> menuItems = new List<SidebarMenuItem>();

    public SidebarMenuStack(string name, string title)
        : base(name, title)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }

    #region Properties

    public DisplayModeEnum DisplayMode { get; set; } = DisplayModeEnum.Auto;

    #endregion

    public IReadOnlyList<SidebarMenuItem> MenuItems => menuItems;

    public void AddMenuItem(SidebarMenuItem item)
    {
        menuItems.Add(item);
    }

    #region Overrides of NavigationStackBase<FlyoutPage>

    protected override Page? ResolveRoute(Uri routeUri)
    {
        // Resolve via absolute URI
        if (!routeUri.IsAbsoluteUri)
            routeUri = this.BuildRoute(routeUri);

        var flyoutPageItem = menuItems.FirstOrDefault(item => this.GetRoutePath(routeUri).Equals(this.GetRoutePath(item.RouteUri)));

        // Try page factory first ...
        if (flyoutPageItem?.PageFactory != null)
            return flyoutPageItem.PageFactory(routeUri);

        // ... or create instance of page type
        if (flyoutPageItem?.PageType != null)
            return Navigation.UIPlatform.GetPage(flyoutPageItem.PageType, routeUri);

        return null;
    }

    protected override TC InitContainer()
    {
        var sidebarMenuContainer = new TC { NavigationStack = this };
        if (sidebarMenuContainer.SidebarMenu == null)
            throw new InvalidOperationException($"No {nameof(SidebarMenu)} found in NavigationContainer.");

        sidebarMenuContainer.SidebarMenu.DisplayMode = DisplayMode;
        sidebarMenuContainer.SidebarMenu.ItemsSource = MenuItems;

        return sidebarMenuContainer;
    }

    public override INavigationStack? RequestStack(string stackName)
    {
        if (stackName.Equals(Name, StringComparison.InvariantCulture))
            return this;

        return null;
    }

    public override void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
    {
        throw new NotSupportedException($"Use .{nameof(AddMenuItem)}() for {nameof(SidebarMenuStack<TC>)} instead.");
    }

    #endregion
}