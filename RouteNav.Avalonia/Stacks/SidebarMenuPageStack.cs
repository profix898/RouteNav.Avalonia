using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.StackContainers;
using static RouteNav.Avalonia.Controls.SidebarMenu;

namespace RouteNav.Avalonia.Stacks;

public class SidebarMenuPageStack : SidebarMenuPageStack<SidebarMenuPageContainer>
{
    public SidebarMenuPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
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
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
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

    #region Overrides of NavigationStackBase<SidebarMenuPage>

    protected override TC InitContainer()
    {
        var sidebarMenuContainer = new TC { NavigationStack = this };
        sidebarMenuContainer.HostControlAttached += () =>
        {
            if (sidebarMenuContainer.SidebarMenu == null)
                throw new InvalidOperationException($"No {nameof(SidebarMenu)} found in NavigationContainer.");

            sidebarMenuContainer.SidebarMenu.DisplayMode = DisplayMode;
            sidebarMenuContainer.SidebarMenu.MenuItemsSource = menuItems.Select(mi => mi.Clone());
        };
        RootPage ??= new LazyValue<Page>(() => new Page());

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
        var pageKey = relativeRoute.Trim('/');
        pages.Set(pageKey, pageFactory);

        // RootPage
        if (String.IsNullOrEmpty(pageKey))
            RootPage = new LazyValue<Page>(() => pageFactory(this.BuildRoute(String.Empty)));
    }

    public override Task<Page> PushAsync(Page page)
    {
        if (page.Equals(CurrentPage))
            return Task.FromResult(CurrentPage);

        var previousPage = CurrentPage;

        pageStack.Clear();
        pageStack.Add(page);
        CurrentPage = page;

        ContainerPage.Value.UpdatePage(CurrentPage);
        OnPageNavigated(previousPage, CurrentPage);

        return Task.FromResult(CurrentPage);
    }

    #endregion
}