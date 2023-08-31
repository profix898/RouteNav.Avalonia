using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Platform;

namespace RouteNav.Avalonia.Controls;

public class SidebarMenu : TabControl, ISafeAreaAware
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

    public static readonly StyledProperty<IBrush?> SidebarBackgroundProperty = SplitView.PaneBackgroundProperty.AddOwner<SidebarMenu>();

    public static readonly StyledProperty<object?> SidebarHeaderProperty = AvaloniaProperty.Register<SidebarMenu, object?>(nameof(SidebarHeader));

    public static readonly StyledProperty<object?> SidebarFooterProperty = AvaloniaProperty.Register<SidebarMenu, object?>(nameof(SidebarFooter));
    
    public static readonly StyledProperty<IBrush?> ContentBackgroundProperty = AvaloniaProperty.Register<SidebarMenu, IBrush?>(nameof(ContentBackground));

    public static readonly StyledProperty<DisplayModeEnum> DisplayModeProperty = AvaloniaProperty.Register<SidebarMenu, DisplayModeEnum>(nameof(InlineThresholdWidth), DisplayModeEnum.Auto);

    public static readonly StyledProperty<int> InlineThresholdWidthProperty = AvaloniaProperty.Register<SidebarMenu, int>(nameof(InlineThresholdWidth), 1005);

    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<SidebarMenu, Thickness>(nameof(SafeAreaPadding));

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        splitView = e.NameScope.Find<SplitView>("PART_NavigationPane");
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
        {
            EnsureSplitViewDisplayMode(Bounds);
        }
        else if (change.Property == SelectedItemProperty)
        {
            if (splitView != null && (splitView.DisplayMode == SplitViewDisplayMode.Overlay || splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay))
                splitView.SetCurrentValue(SplitView.IsPaneOpenProperty, false);
        }
        else if (change.Property == SafeAreaPaddingProperty || change.Property == PaddingProperty || change.Property == ItemsSourceProperty)
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

    protected virtual void UpdateContentSafeAreaPadding()
    {
        if (ItemsSource != null && ItemsPanelRoot != null)
        {
            var remainingSafeArea = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);

            foreach (var child in ItemsPanelRoot.Children)
            {
                if (child is ISafeAreaAware safeAreaAwareChild)
                    safeAreaAwareChild.SafeAreaPadding = remainingSafeArea;
                else if (child is TemplatedControl templatedControl)
                    templatedControl.Padding = templatedControl.Padding.ApplySafeAreaPadding(remainingSafeArea);
                else
                    Padding = Padding.ApplySafeAreaPadding(remainingSafeArea); // Fallback: Add padding to parent
            }
        }
    }
}
