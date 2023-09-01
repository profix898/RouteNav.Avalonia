using System;
using System.Collections.Generic;
using System.Linq;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Platform;
using RouteNav.Avalonia.StackContainers;
using static RouteNav.Avalonia.Controls.SidebarMenu;

namespace RouteNav.Avalonia.Stacks;

public class SidebarMenuPageStack : SidebarMenuPageStack<SidebarMenuPageContainer>
{
    public SidebarMenuPageStack(string name, string title)
        : base(name, title)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }
}

public class SidebarMenuPageStack<TC> : NavigationStackBase<TC>, INavigationStack, ISidebarMenuPageStack
    where TC : SidebarMenuPageContainer, new()
{
    private readonly List<SidebarMenuItem> menuItems = new List<SidebarMenuItem>();

    public SidebarMenuPageStack(string name, string title)
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

        var menuItem = menuItems.FirstOrDefault(item => this.EqualsRoutePath(routeUri, item.RouteUri));

        // Try page factory first ...
        if (menuItem?.PageFactory != null)
            return menuItem.PageFactory(routeUri);

        // ... or create instance of page type
        if (menuItem?.PageType != null)
            return Navigation.UIPlatform.GetPage(menuItem.PageType, routeUri);

        return null;
    }

    protected override TC InitContainer()
    {
        var sidebarMenuContainer = new TC { NavigationStack = this };
        sidebarMenuContainer.HostControlAttached += () =>
        {
            if (sidebarMenuContainer.SidebarMenu == null)
                throw new InvalidOperationException($"No {nameof(SidebarMenu)} found in NavigationContainer.");

            sidebarMenuContainer.SidebarMenu.DisplayMode = DisplayMode;
            sidebarMenuContainer.SidebarMenu.MenuItemsSource = menuItems;
        };
        RootPage = new LazyValue<Page>(() => new Page());

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
        throw new NotSupportedException($"Use .{nameof(AddMenuItem)}() for {nameof(SidebarMenuPageStack)} instead.");
    }

    #endregion
}