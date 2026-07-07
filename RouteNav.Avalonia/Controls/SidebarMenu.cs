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
/// <see cref="global::Avalonia.Controls.DrawerPage"/> internally (hosted in its template), so the
/// native V12 drawer theming applies automatically and can be overridden the same way as any other
/// V12 control (via the <c>DrawerPage</c> control theme or the Fluent drawer resource keys).
/// The control itself exposes only RouteNav's menu/routing concepts, keeping the API free of the
/// framework's own page-navigation members.
/// </summary>
[TemplatePart("PART_Drawer", typeof(global::Avalonia.Controls.DrawerPage))]
[TemplatePart("PART_MenuItemList", typeof(ListBox))]
public sealed class SidebarMenu : TemplatedControl, ISafeAreaAware
{
    #region DisplayModeEnum

    public enum DisplayModeEnum
    {
        /// <summary>
        /// Inline (width &gt;= <see cref="InlineThresholdWidth"/>) or Overlay (width &lt; <see cref="InlineThresholdWidth"/>).
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

    private global::Avalonia.Controls.DrawerPage? drawer;
    private ListBox? menuList;
    private bool suppressSelectionChanged;

    public static readonly StyledProperty<object?> SidebarHeaderProperty = AvaloniaProperty.Register<SidebarMenu, object?>(nameof(SidebarHeader));

    public static readonly StyledProperty<object?> SidebarFooterProperty = AvaloniaProperty.Register<SidebarMenu, object?>(nameof(SidebarFooter));

    public static readonly StyledProperty<DisplayModeEnum> DisplayModeProperty = AvaloniaProperty.Register<SidebarMenu, DisplayModeEnum>(nameof(DisplayMode), DisplayModeEnum.Auto);

    public static readonly StyledProperty<int> InlineThresholdWidthProperty = AvaloniaProperty.Register<SidebarMenu, int>(nameof(InlineThresholdWidth), 1005);

    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<SidebarMenu, Thickness>(nameof(SafeAreaPadding));

    public static readonly DirectProperty<SidebarMenu, IEnumerable<SidebarMenuItem>> MenuItemsSourceProperty =
        AvaloniaProperty.RegisterDirect<SidebarMenu, IEnumerable<SidebarMenuItem>>(nameof(MenuItemsSource), s => s.MenuItems, (s, items) =>
        {
            s.MenuItems.Clear();
            s.MenuItems.AddRange(items);
            s.RefreshMenuItems();
        });

    public static readonly RoutedEvent<RoutedEventArgs> SelectedMenuItemChangedEvent = RoutedEvent.Register<SidebarMenu, RoutedEventArgs>(nameof(SelectedMenuItemChanged), RoutingStrategies.Bubble);

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

    public DisplayModeEnum DisplayMode
    {
        get { return GetValue(DisplayModeProperty); }
        set { SetValue(DisplayModeProperty, value); }
    }

    /// <summary>Width threshold below which <see cref="DisplayModeEnum.Auto"/> switches from inline to overlay.</summary>
    public int InlineThresholdWidth
    {
        get { return GetValue(InlineThresholdWidthProperty); }
        set { SetValue(InlineThresholdWidthProperty, value); }
    }

    #region Implementation of ISafeAreaAware

    public Thickness SafeAreaPadding
    {
        get { return GetValue(SafeAreaPaddingProperty); }
        set { SetValue(SafeAreaPaddingProperty, value); }
    }

    #endregion

    [Content]
    public List<SidebarMenuItem> MenuItems { get; } = new List<SidebarMenuItem>();

    public IEnumerable<SidebarMenuItem> MenuItemsSource
    {
        get { return GetValue(MenuItemsSourceProperty); }
        set { SetValue(MenuItemsSourceProperty, value); }
    }

    public event EventHandler<RoutedEventArgs>? SelectedMenuItemChanged
    {
        add => AddHandler(SelectedMenuItemChangedEvent, value);
        remove => RemoveHandler(SelectedMenuItemChangedEvent, value);
    }

    public SidebarMenuItem? SelectedMenuItem { get; private set; }

    /// <summary>The page shown in the detail area (forwarded to <c>DrawerPage.Content</c> via the template).</summary>
    public Page? Page
    {
        get { return GetValue(PageProperty); }
        set { SetValue(PageProperty, value); }
    }

    public INavigationStack? NavigationStack { get; internal set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        drawer = e.NameScope.Find<global::Avalonia.Controls.DrawerPage>("PART_Drawer");

        if (menuList != null)
            menuList.SelectionChanged -= MenuList_OnSelectionChanged;
        menuList = e.NameScope.Find<ListBox>("PART_MenuItemList");
        if (menuList != null)
        {
            menuList.ItemsSource = MenuItems.ToList();
            menuList.SelectionChanged += MenuList_OnSelectionChanged;
        }

        ApplyDisplayMode();
        UpdateContentSafeAreaPadding();
        SyncSelectionToCurrentPage();
    }

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

        menuList.ItemsSource = MenuItems.ToList();
        SyncSelectionToCurrentPage();
    }

    private void ApplyDisplayMode()
    {
        if (drawer == null)
            return;

        switch (DisplayMode)
        {
            case DisplayModeEnum.Auto:
                // Inline (side-by-side) when wide; automatically collapses to an overlay below the threshold.
                drawer.DrawerBehavior = global::Avalonia.Controls.DrawerBehavior.Auto;
                drawer.DrawerLayoutBehavior = global::Avalonia.Controls.DrawerLayoutBehavior.Split;
                drawer.DrawerBreakpointLength = InlineThresholdWidth;
                break;
            case DisplayModeEnum.Inline:
                drawer.DrawerBehavior = global::Avalonia.Controls.DrawerBehavior.Locked;
                drawer.DrawerLayoutBehavior = global::Avalonia.Controls.DrawerLayoutBehavior.Split;
                drawer.DrawerBreakpointLength = 0;
                break;
            case DisplayModeEnum.Overlay:
                drawer.DrawerBehavior = global::Avalonia.Controls.DrawerBehavior.Auto;
                drawer.DrawerLayoutBehavior = global::Avalonia.Controls.DrawerLayoutBehavior.Overlay;
                drawer.DrawerBreakpointLength = 0;
                break;
            case DisplayModeEnum.CompactOverlay:
                drawer.DrawerBehavior = global::Avalonia.Controls.DrawerBehavior.Auto;
                drawer.DrawerLayoutBehavior = global::Avalonia.Controls.DrawerLayoutBehavior.CompactOverlay;
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
            if (Page.PageQuery.TryGetValue("routeUri", out var routeUriString))
            {
                var routeUri = new Uri(routeUriString);
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
            && drawer.DrawerBehavior != global::Avalonia.Controls.DrawerBehavior.Locked
            && (drawer.DrawerLayoutBehavior == global::Avalonia.Controls.DrawerLayoutBehavior.Overlay
                || drawer.DrawerLayoutBehavior == global::Avalonia.Controls.DrawerLayoutBehavior.CompactOverlay))
            drawer.SetCurrentValue(global::Avalonia.Controls.DrawerPage.IsOpenProperty, false);

        if (menuList != null && menuList.SelectedIndex >= 0 && menuList.SelectedIndex < MenuItems.Count)
            SelectedMenuItem = MenuItems[menuList.SelectedIndex];
        else
            SelectedMenuItem = null;

        if (suppressSelectionChanged)
            return;

        RaiseEvent(new RoutedEventArgs(SelectedMenuItemChangedEvent));
    }
}
