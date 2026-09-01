using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.StackContainers;
using static RouteNav.Avalonia.Controls.SidebarMenu;

namespace RouteNav.Avalonia.Stacks;

/// <summary>A navigation stack whose pages are selected from a sidebar/drawer menu.</summary>
public class SidebarMenuPageStack : SidebarMenuPageStack<SidebarMenuPageContainer>
{
    /// <summary>Initializes a new instance of the <see cref="SidebarMenuPageStack" /> class.</summary>
    public SidebarMenuPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }
}

/// <summary>A navigation stack whose pages are selected from a sidebar/drawer menu.</summary>
/// <typeparam name="TC">The navigation container type.</typeparam>
public class SidebarMenuPageStack<TC> : NavigationStackBase<TC>, INavigationStack, ISidebarMenuPageStack
    where TC : SidebarMenuPageContainer, new()
{
    private readonly List<SidebarMenuItem> menuItems = new List<SidebarMenuItem>();

    /// <summary>Initializes a new instance of the <see cref="SidebarMenuPageStack{TC}" /> class.</summary>
    public SidebarMenuPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }

    #region Properties

    /// <summary>Gets or sets how the drawer is displayed relative to the content.</summary>
    public DisplayModeEnum DisplayMode { get; set; } = DisplayModeEnum.Auto;

    #endregion

    /// <summary>Gets the menu items that make up the sidebar.</summary>
    public IReadOnlyList<SidebarMenuItem> MenuItems => menuItems;

    /// <summary>Adds a menu item to the sidebar.</summary>
    public void AddMenuItem(SidebarMenuItem item)
    {
        menuItems.Add(item);
    }

    #region Overrides of NavigationStackBase<SidebarMenuPage>

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override INavigationStack? RequestStack(string stackName)
    {
        if (stackName.Equals(Name, StringComparison.Ordinal))
            return this;

        return null;
    }

    /// <inheritdoc />
    public override void AddPage(string relativeRoute, Func<Uri, Page> pageFactory)
    {
        var pageKey = relativeRoute.Trim('/');
        pages.Set(pageKey, new RegisteredRoute(pageKey, null, pageFactory));

        // RootPage
        if (String.IsNullOrEmpty(pageKey))
            RootPage = new LazyValue<Page>(() => pageFactory(this.BuildRoute(String.Empty)));
    }

    /// <inheritdoc />
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
