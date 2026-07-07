using Avalonia;
using Avalonia.Controls;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.StackContainers;

/// <summary>A navigation container that hosts pages in a <see cref="TabControl"/>.</summary>
public class TabbedPageContainer : NavigationContainer
{
    /// <summary>Defines the <see cref="TabControlName"/> property.</summary>
    public static readonly StyledProperty<string> TabControlNameProperty = AvaloniaProperty.Register<TabbedPageContainer, string>(nameof(TabControlName), "TabControl");

    /// <summary>Initializes a new instance of the <see cref="TabbedPageContainer"/> class.</summary>
    public TabbedPageContainer()
    {
        TabControl = new TabControl();
        TabControl.Classes.Add("RouteNavTabbedPageTabs");
        TabControl.SetValue(Panel.ZIndexProperty, 1);

        var headerBackground = new Border { Height = 38, VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top };
        headerBackground.Classes.Add("RouteNavTabbedPageHeaderBackground");

        var host = new Grid();
        host.Children.Add(headerBackground);
        host.Children.Add(TabControl);

        RegisterScopedControl(this, TabControlName, TabControl);
        Content = host;
    }

    /// <summary>Gets or sets the scoped name used to resolve the tab control.</summary>
    public string TabControlName
    {
        get { return GetValue(TabControlNameProperty); }
        set { SetValue(TabControlNameProperty, value); }
    }

    /// <summary>Gets the resolved tab control.</summary>
    public TabControl? TabControl { get; private set; }

    /// <inheritdoc />
    public override void UpdatePage(Page page)
    {
        if (TabControl != null)
            TabControl.SelectedItem = FindTabItem(TabControl, page);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (TabControl != null)
            TabControl.SelectionChanged -= TabControl_OnSelectionChanged;
        TabControl = this.GetControl<TabControl>(TabControlName);
        if (TabControl != null)
            TabControl.SelectionChanged += TabControl_OnSelectionChanged;

        OnHostControlAttached();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TabControlNameProperty)
        {
            if (TabControl != null)
                TabControl.SelectionChanged -= TabControl_OnSelectionChanged;
            TabControl = this.GetControl<TabControl>(TabControlName);
            if (TabControl != null)
                TabControl.SelectionChanged += TabControl_OnSelectionChanged;

            OnHostControlAttached();
        }
    }

    /// <inheritdoc />
    protected override void UpdateContentSafeAreaPadding()
    {
        if (TabControl != null)
            TabControl.Padding = TabControl.Padding.ApplySafeAreaPadding(Padding.GetRemainingSafeAreaPadding(SafeAreaPadding));
    }

    private void TabControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (NavigationStack is TabbedPageStack<TabbedPageContainer> tabbedPageStack && TabControl?.SelectedItem is TabItem tabItem)
            tabbedPageStack.SetCurrentPage(FindPage(tabItem));
    }

    #region Private

    internal static Page? FindPage(TabItem tabItem)
    {
        if (tabItem.Content is Page tabPage)
            return tabPage;

        return null;
    }

    internal static TabItem? FindTabItem(TabControl tabControl, Page page)
    {
        foreach (var item in tabControl.Items)
        {
            if (item is not TabItem tabItem)
                continue;

            if (tabItem.Content is not Page tabPage)
                continue;

            if (tabPage.Equals(page))
                return tabItem;
        }

        return null;
    }

    #endregion
}
