using Avalonia;
using Avalonia.Controls;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Internal;

namespace RouteNav.Avalonia.StackContainers;

public class SidebarMenuContainer : NavigationContainer
{
    public static readonly StyledProperty<string> SidebarMenuNameProperty = AvaloniaProperty.Register<SidebarMenuContainer, string>(nameof(SidebarMenuName));

    public string SidebarMenuName
    {
        get { return GetValue(SidebarMenuNameProperty); }
        set { SetValue(SidebarMenuNameProperty, value); }
    }

    public SidebarMenu? SidebarMenu { get; private set; }

    public override void UpdatePage(Page page)
    {
        if (SidebarMenu != null)
            SidebarMenu.SelectedItem = TabbedPageContainer.FindTabItem(SidebarMenu, page);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SidebarMenuNameProperty)
            SidebarMenu = this.Get<SidebarMenu>(SidebarMenuName);
    }

    protected override void UpdateContentSafeAreaPadding()
    {
        if (SidebarMenu != null)
            SidebarMenu.SafeAreaPadding = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);
    }
}