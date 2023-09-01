using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Platform;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Controls;

[TemplatePart("PART_SidebarMenuPane", typeof(SplitView))]
[TemplatePart("PART_MenuItemList", typeof(ListBox))]
[TemplatePart("PART_NavigationContent", typeof(ContentPresenter))]
public sealed class SidebarMenu : TemplatedControl, ISafeAreaAware
{
    #region DisplayModeEnum

    public enum DisplayModeEnum
    {
        /// <summary>
        /// Inline (width &lt; InlineThresholdWidth) or Overlay (width &gt; InlineThresholdWidth)
        /// </summary>
        Auto,
        /// <summary>
        /// Menu is displayed next to content
        /// </summary>
        Inline,
        /// <summary>
        /// Menu is displayed on flyout above content
        /// </summary>
        Overlay,
        /// <summary>
        /// Menu is displayed on flyout above content,
        /// a small part is still visible when collapsed
        /// </summary>
        CompactOverlay
    }
    
    #endregion

    private SplitView? splitView;
    private ListBox? listBox;
    private ContentPresenter? contentPresenter;

    public static readonly StyledProperty<IBrush?> SidebarBackgroundProperty = SplitView.PaneBackgroundProperty.AddOwner<SidebarMenu>();

    public static readonly StyledProperty<object?> SidebarHeaderProperty = AvaloniaProperty.Register<SidebarMenu, object?>(nameof(SidebarHeader));

    public static readonly StyledProperty<object?> SidebarFooterProperty = AvaloniaProperty.Register<SidebarMenu, object?>(nameof(SidebarFooter));
    
    public static readonly StyledProperty<IBrush?> ContentBackgroundProperty = AvaloniaProperty.Register<SidebarMenu, IBrush?>(nameof(ContentBackground));

    public static readonly StyledProperty<DisplayModeEnum> DisplayModeProperty = AvaloniaProperty.Register<SidebarMenu, DisplayModeEnum>(nameof(InlineThresholdWidth), DisplayModeEnum.Auto);

    public static readonly StyledProperty<int> InlineThresholdWidthProperty = AvaloniaProperty.Register<SidebarMenu, int>(nameof(InlineThresholdWidth), 1005);

    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<SidebarMenu, Thickness>(nameof(SafeAreaPadding));

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty = ContentControl.HorizontalContentAlignmentProperty.AddOwner<SidebarMenu>();

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty = ContentControl.VerticalContentAlignmentProperty.AddOwner<SidebarMenu>();

    public static readonly DirectProperty<SidebarMenu, IEnumerable<SidebarMenuItem>> MenuItemsSourceProperty =
        AvaloniaProperty.RegisterDirect<SidebarMenu, IEnumerable<SidebarMenuItem>>(nameof(MenuItemsSource), s => s.MenuItems, (s, items) =>
        {
            s.MenuItems.Clear();
            s.MenuItems.AddRange(items);
        });

    public static readonly RoutedEvent<RoutedEventArgs> SelectedMenuItemChangedEvent = RoutedEvent.Register<SidebarMenu, RoutedEventArgs>(nameof(SelectedMenuItemChanged), RoutingStrategies.Bubble);

    public static readonly StyledProperty<Page?> PageProperty = AvaloniaProperty.Register<SidebarMenu, Page?>(nameof(Page));

    public IBrush? SidebarBackground
    {
        get { return GetValue(SidebarBackgroundProperty); }
        set { SetValue(SidebarBackgroundProperty, value); }
    }

    public object? SidebarHeader
    {
        get { return GetValue(SidebarHeaderProperty); }
        set { SetValue(SidebarHeaderProperty, value); }
    }

    public object? SidebarFooter
    {
        get { return GetValue(SidebarFooterProperty); }
        set { SetValue(SidebarFooterProperty, value); }
    }

    public IBrush? ContentBackground
    {
        get { return GetValue(ContentBackgroundProperty); }
        set { SetValue(ContentBackgroundProperty, value); }
    }

    public DisplayModeEnum DisplayMode
    {
        get { return GetValue(DisplayModeProperty); }
        set { SetValue(DisplayModeProperty, value); }
    }

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

    public HorizontalAlignment HorizontalContentAlignment
    {
        get { return GetValue(HorizontalContentAlignmentProperty); }
        set { SetValue(HorizontalContentAlignmentProperty, value); }
    }

    public VerticalAlignment VerticalContentAlignment
    {
        get { return GetValue(VerticalContentAlignmentProperty); }
        set { SetValue(VerticalContentAlignmentProperty, value); }
    }

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

    public Page? Page
    {
        get { return GetValue(PageProperty); }
        set { SetValue(PageProperty, value); }
    }

    public INavigationStack? NavigationStack { get; internal set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        splitView = e.NameScope.Find<SplitView>("PART_SidebarMenuPane");

        if (listBox != null)
            listBox.SelectionChanged -= ListBox_OnSelectionChanged;
        listBox = e.NameScope.Find<ListBox>("PART_MenuItemList");
        if (listBox != null)
        {
            listBox.SelectionMode = NavigationStack != null ? SelectionMode.AlwaysSelected : SelectionMode.Single;
            listBox.SelectedIndex = 0; // Initial selection
            listBox.SelectionChanged += ListBox_OnSelectionChanged;
        }

        contentPresenter = e.NameScope.Find<ContentPresenter>("PART_NavigationContent");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty && splitView != null)
        {
            var (_, newBounds) = change.GetOldAndNewValue<Rect>();
            EnsureSplitViewDisplayMode(newBounds);
        }
        else if ((change.Property == DisplayModeProperty || change.Property == InlineThresholdWidthProperty) && splitView != null)
            EnsureSplitViewDisplayMode(Bounds);
        else if (change.Property == PageProperty && contentPresenter != null)
        {
            contentPresenter.Content = Page;

            // Validate (and potentially fix) listbox selection
            if (Page != null && NavigationStack != null && listBox != null)
            {
                if (Page.PageQuery.TryGetValue("routeUri", out var routeUriString))
                {
                    var routeUri = new Uri(routeUriString);
                    var idx = MenuItems.FindIndex(item => NavigationStack.EqualsRoutePath(NavigationStack.BuildRoute(item.RouteUri), routeUri));
                    if (listBox.SelectedIndex != idx)
                        listBox.SelectedIndex = idx;
                }
                else
                    listBox.SelectedIndex = -1;
            }
        }
        else if (change.Property == SafeAreaPaddingProperty || change.Property == PaddingProperty)
            UpdateContentSafeAreaPadding();
    }

    private void EnsureSplitViewDisplayMode(Rect bounds)
    {
        if (splitView == null)
            return;

        switch (DisplayMode)
        {
            case DisplayModeEnum.Auto:
                if (bounds.Width < InlineThresholdWidth)
                {
                    splitView.DisplayMode = SplitViewDisplayMode.Overlay;
                    splitView.IsPaneOpen = false;
                }
                else
                {
                    splitView.DisplayMode = SplitViewDisplayMode.Inline;
                    splitView.IsPaneOpen = true;
                }
                break;
            case DisplayModeEnum.Inline:
                splitView.DisplayMode = SplitViewDisplayMode.Inline;
                splitView.IsPaneOpen = true;
                break;
            case DisplayModeEnum.Overlay:
                splitView.DisplayMode = SplitViewDisplayMode.Overlay;
                splitView.IsPaneOpen = false;
                break;
            case DisplayModeEnum.CompactOverlay:
                splitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                splitView.IsPaneOpen = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateContentSafeAreaPadding()
    {
        var remainingSafeArea = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);

        if (contentPresenter != null && contentPresenter.Content is ISafeAreaAware safeAreaAwareChild)
            safeAreaAwareChild.SafeAreaPadding = remainingSafeArea;
        else if (splitView != null)
            splitView.Padding = splitView.Padding.ApplySafeAreaPadding(remainingSafeArea);
    }

    private void ListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (splitView != null && (splitView.DisplayMode == SplitViewDisplayMode.Overlay || splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay))
            splitView.SetCurrentValue(SplitView.IsPaneOpenProperty, false);

        if (listBox != null && listBox.SelectedIndex >= 0)
            SelectedMenuItem = MenuItems[listBox.SelectedIndex];
        else
        {
            SelectedMenuItem = null;
            Page = null; // Clear page
        }

        RaiseEvent(new RoutedEventArgs(SelectedMenuItemChangedEvent));
    }
}
