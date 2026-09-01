using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Platform;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Controls;

/// <summary>
/// A routing-aware sidebar/drawer menu. It <b>composes</b> Avalonia 12's native
/// <see cref="global::Avalonia.Controls.DrawerPage" /> internally (hosted in its template), so the
/// native V12 drawer theming applies automatically and can be overridden the same way as any other
/// V12 control (via the <c>DrawerPage</c> control theme or the Fluent drawer resource keys).
/// The control itself exposes only RouteNav's menu/routing concepts, keeping the API free of the
/// framework's own page-navigation members.
/// </summary>
[TemplatePart("PART_Drawer", typeof(DrawerPage))]
[TemplatePart("PART_MenuItemList", typeof(ListBox))]
public sealed class SidebarMenu : TemplatedControl, ISafeAreaAware
{
    #region DisplayModeEnum

    /// <summary>Defines how the sidebar drawer is displayed relative to the content.</summary>
    public enum DisplayModeEnum
    {
        /// <summary>
        /// Inline (width &gt;= <see cref="InlineThresholdWidth" />) or Overlay (width &lt; <see cref="InlineThresholdWidth" />).
        /// </summary>
        Auto,

        /// <summary>
        /// Menu is permanently displayed next to the content.
        /// </summary>
        Inline,

        /// <summary>
        /// Menu is displayed as an overlay above the content.
        /// </summary>
        Overlay,

        /// <summary>
        /// Menu is displayed as an overlay above the content; a compact rail stays visible when collapsed.
        /// </summary>
        CompactOverlay
    }

    #endregion

    private DrawerPage? drawer;
    private ListBox? menuList;
    private bool suppressSelectionChanged;

    /// <summary>Defines the <see cref="SidebarHeader" /> property.</summary>
    public static readonly StyledProperty<object?> SidebarHeaderProperty = AvaloniaProperty.Register<SidebarMenu, object?>(nameof(SidebarHeader));

    /// <summary>Defines the <see cref="SidebarFooter" /> property.</summary>
    public static readonly StyledProperty<object?> SidebarFooterProperty = AvaloniaProperty.Register<SidebarMenu, object?>(nameof(SidebarFooter));

    /// <summary>Defines the <see cref="DisplayMode" /> property.</summary>
    public static readonly StyledProperty<DisplayModeEnum> DisplayModeProperty = AvaloniaProperty.Register<SidebarMenu, DisplayModeEnum>(nameof(DisplayMode));

    /// <summary>Defines the <see cref="InlineThresholdWidth" /> property.</summary>
    public static readonly StyledProperty<int> InlineThresholdWidthProperty = AvaloniaProperty.Register<SidebarMenu, int>(nameof(InlineThresholdWidth), 1005);

    /// <summary>Defines the <see cref="SafeAreaPadding" /> property.</summary>
    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<SidebarMenu, Thickness>(nameof(SafeAreaPadding));

    /// <summary>Defines the <see cref="MenuItemsSource" /> property.</summary>
    public static readonly DirectProperty<SidebarMenu, IEnumerable<SidebarMenuItem>> MenuItemsSourceProperty =
        AvaloniaProperty.RegisterDirect<SidebarMenu, IEnumerable<SidebarMenuItem>>(nameof(MenuItemsSource), s => s.MenuItems, (s, items) =>
        {
            s.MenuItems.Clear();
            s.MenuItems.AddRange(items);
            s.RefreshMenuItems();
        });

    /// <summary>Defines the <see cref="SelectedMenuItemChanged" /> routed event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> SelectedMenuItemChangedEvent =
        RoutedEvent.Register<SidebarMenu, RoutedEventArgs>(nameof(SelectedMenuItemChanged), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="Page" /> property.</summary>
    public static readonly StyledProperty<Page?> PageProperty = AvaloniaProperty.Register<SidebarMenu, Page?>(nameof(Page));

    /// <summary>Content displayed at the top of the drawer pane (forwarded to <c>DrawerPage.DrawerHeader</c>).</summary>
    public object? SidebarHeader
    {
        get { return GetValue(SidebarHeaderProperty); }
        set { SetValue(SidebarHeaderProperty, value); }
    }

    /// <summary>Content displayed at the bottom of the drawer pane (forwarded to <c>DrawerPage.DrawerFooter</c>).</summary>
    public object? SidebarFooter
    {
        get { return GetValue(SidebarFooterProperty); }
        set { SetValue(SidebarFooterProperty, value); }
    }

    /// <summary>Gets or sets how the drawer is displayed relative to the content.</summary>
    public DisplayModeEnum DisplayMode
    {
        get { return GetValue(DisplayModeProperty); }
        set { SetValue(DisplayModeProperty, value); }
    }

    /// <summary>Width threshold below which <see cref="DisplayModeEnum.Auto" /> switches from inline to overlay.</summary>
    public int InlineThresholdWidth
    {
        get { return GetValue(InlineThresholdWidthProperty); }
        set { SetValue(InlineThresholdWidthProperty, value); }
    }

    #region Implementation of ISafeAreaAware

    /// <inheritdoc />
    public Thickness SafeAreaPadding
    {
        get { return GetValue(SafeAreaPaddingProperty); }
        set { SetValue(SafeAreaPaddingProperty, value); }
    }

    #endregion

    /// <summary>Gets the menu items shown in the drawer.</summary>
    [Content]
    public List<SidebarMenuItem> MenuItems { get; } = new List<SidebarMenuItem>();

    /// <summary>Gets or sets the source collection used to populate <see cref="MenuItems" />.</summary>
    public IEnumerable<SidebarMenuItem> MenuItemsSource
    {
        get { return GetValue(MenuItemsSourceProperty); }
        set { SetValue(MenuItemsSourceProperty, value); }
    }

    /// <summary>Raised when the selected menu item changes.</summary>
    public event EventHandler<RoutedEventArgs>? SelectedMenuItemChanged
    {
        add => AddHandler(SelectedMenuItemChangedEvent, value);
        remove => RemoveHandler(SelectedMenuItemChangedEvent, value);
    }

    /// <summary>Gets the currently selected menu item, or <c>null</c> if none is selected.</summary>
    public SidebarMenuItem? SelectedMenuItem { get; private set; }

    /// <summary>The page shown in the detail area (forwarded to <c>DrawerPage.Content</c> via the template).</summary>
    public Page? Page
    {
        get { return GetValue(PageProperty); }
        set { SetValue(PageProperty, value); }
    }

    /// <summary>Gets the navigation stack this menu drives.</summary>
    public INavigationStack? NavigationStack { get; internal set; }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        drawer = e.NameScope.Find<DrawerPage>("PART_Drawer");

        if (menuList != null)
            menuList.SelectionChanged -= MenuList_OnSelectionChanged;
        menuList = e.NameScope.Find<ListBox>("PART_MenuItemList");
        if (menuList != null)
        {
            menuList.SelectionChanged += MenuList_OnSelectionChanged;
            BindMenuItems();
        }

        ApplyDisplayMode();
        UpdateContentSafeAreaPadding();
        SyncSelectionToCurrentPage();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PageProperty)
            SyncSelectionToCurrentPage();
        else if (change.Property == DisplayModeProperty || change.Property == InlineThresholdWidthProperty)
            ApplyDisplayMode();
        else if (change.Property == SafeAreaPaddingProperty || change.Property == PaddingProperty)
            UpdateContentSafeAreaPadding();
    }

    private void RefreshMenuItems()
    {
        if (menuList == null)
            return;

        BindMenuItems();
        SyncSelectionToCurrentPage();
    }

    private void BindMenuItems()
    {
        if (menuList != null)
            menuList.ItemsSource = MenuItems.ToList();
    }

    private void ApplyDisplayMode()
    {
        if (drawer == null)
            return;

        switch (DisplayMode)
        {
            case DisplayModeEnum.Auto:
                // Inline (side-by-side) when wide; automatically collapses to an overlay below the threshold.
                drawer.DrawerBehavior = DrawerBehavior.Auto;
                drawer.DrawerLayoutBehavior = DrawerLayoutBehavior.Split;
                drawer.DrawerBreakpointLength = InlineThresholdWidth;
                break;
            case DisplayModeEnum.Inline:
                drawer.DrawerBehavior = DrawerBehavior.Locked;
                drawer.DrawerLayoutBehavior = DrawerLayoutBehavior.Split;
                drawer.DrawerBreakpointLength = 0;
                break;
            case DisplayModeEnum.Overlay:
                drawer.DrawerBehavior = DrawerBehavior.Auto;
                drawer.DrawerLayoutBehavior = DrawerLayoutBehavior.Overlay;
                drawer.DrawerBreakpointLength = 0;
                break;
            case DisplayModeEnum.CompactOverlay:
                drawer.DrawerBehavior = DrawerBehavior.Auto;
                drawer.DrawerLayoutBehavior = DrawerLayoutBehavior.CompactOverlay;
                drawer.DrawerBreakpointLength = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateContentSafeAreaPadding()
    {
        if (drawer != null)
            drawer.SafeAreaPadding = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);
    }

    private void SyncSelectionToCurrentPage()
    {
        if (menuList == null || Page == null || NavigationStack == null || MenuItems.Count == 0)
            return;

        suppressSelectionChanged = true;
        try
        {
            if (Page.RouteUri != null)
            {
                var routeUri = Page.RouteUri;
                var idx = MenuItems.FindIndex(item => NavigationStack.EqualsRoutePath(NavigationStack.BuildRoute(item.RouteUri), routeUri));
                if (menuList.SelectedIndex != idx)
                    menuList.SelectedIndex = idx;
            }
            else
                menuList.SelectedIndex = -1;
        }
        finally
        {
            suppressSelectionChanged = false;
        }
    }

    private void MenuList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Close the drawer after a selection when it is shown as an overlay/flyout pane.
        if (drawer is { IsOpen: true }
            && drawer.DrawerBehavior != DrawerBehavior.Locked
            && (drawer.DrawerLayoutBehavior == DrawerLayoutBehavior.Overlay
                || drawer.DrawerLayoutBehavior == DrawerLayoutBehavior.CompactOverlay))
            drawer.SetCurrentValue(DrawerPage.IsOpenProperty, false);

        if (menuList != null && menuList.SelectedIndex >= 0 && menuList.SelectedIndex < MenuItems.Count)
            SelectedMenuItem = MenuItems[menuList.SelectedIndex];
        else
            SelectedMenuItem = null;

        if (suppressSelectionChanged)
            return;

        RaiseEvent(new RoutedEventArgs(SelectedMenuItemChangedEvent));
    }
}
