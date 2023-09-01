using Avalonia;
using Avalonia.Controls;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.StackContainers;

public class TabbedPageContainer : NavigationContainer
{
    public static readonly StyledProperty<string> TabControlNameProperty = AvaloniaProperty.Register<TabbedPageContainer, string>(nameof(TabControlName), "TabControl");

    public TabbedPageContainer()
    {
        RegisterScopedControl(this, TabControlName, Content = new TabControl());
    }

    public string TabControlName
    {
        get { return GetValue(TabControlNameProperty); }
        set { SetValue(TabControlNameProperty, value); }
    }

    public TabControl? TabControl { get; private set; }

    public override void UpdatePage(Page page)
    {
        if (TabControl != null)
            TabControl.SelectedItem = FindTabItem(TabControl, page);
    }

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

    private static Page? FindPage(TabItem tabItem)
    {
        if (tabItem.Content is Page tabPage)
            return tabPage;

        return null;
    }

    private static TabItem? FindTabItem(TabControl tabControl, Page page)
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
