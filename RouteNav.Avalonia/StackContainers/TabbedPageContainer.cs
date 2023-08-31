using Avalonia;
using Avalonia.Controls;
using RouteNav.Avalonia.Internal;

namespace RouteNav.Avalonia.StackContainers;

public class TabbedPageContainer : NavigationContainer
{
    public static readonly StyledProperty<string> TabControlNameProperty = AvaloniaProperty.Register<TabbedPageContainer, string>(nameof(TabControlName));

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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TabControlNameProperty)
            TabControl = this.Get<TabControl>(TabControlName);
    }

    protected override void UpdateContentSafeAreaPadding()
    {
        if (TabControl != null)
            TabControl.Padding = TabControl.Padding.ApplySafeAreaPadding(Padding.GetRemainingSafeAreaPadding(SafeAreaPadding));
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
}
