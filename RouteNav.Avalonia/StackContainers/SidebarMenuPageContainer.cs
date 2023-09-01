using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.StackContainers;

public class SidebarMenuPageContainer : NavigationContainer
{
    public static readonly StyledProperty<string> SidebarMenuNameProperty = AvaloniaProperty.Register<SidebarMenuPageContainer, string>(nameof(SidebarMenuName), "SidebarMenu");

    public SidebarMenuPageContainer()
    {
        RegisterScopedControl(this, SidebarMenuName, Content = new SidebarMenu());
    }

    public string SidebarMenuName
    {
        get { return GetValue(SidebarMenuNameProperty); }
        set { SetValue(SidebarMenuNameProperty, value); }
    }

    public SidebarMenu? SidebarMenu { get; private set; }

    public override void UpdatePage(Page page)
    {
        if (SidebarMenu != null && NavigationStack != null)
            SidebarMenu.Page = page;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (SidebarMenu != null)
            SidebarMenu.SelectedMenuItemChanged -= SidebarMenu_OnSelectedMenuItemChanged;
        SidebarMenu = this.GetControl<SidebarMenu>(SidebarMenuName);
        SidebarMenu.NavigationStack = NavigationStack;
        if (SidebarMenu != null)
            SidebarMenu.SelectedMenuItemChanged += SidebarMenu_OnSelectedMenuItemChanged;
        
        OnHostControlAttached();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SidebarMenuNameProperty)
        {
            if (SidebarMenu != null)
                SidebarMenu.SelectedMenuItemChanged -= SidebarMenu_OnSelectedMenuItemChanged;
            SidebarMenu = this.GetControl<SidebarMenu>(SidebarMenuName);
            SidebarMenu.NavigationStack = NavigationStack;
            if (SidebarMenu != null)
                SidebarMenu.SelectedMenuItemChanged += SidebarMenu_OnSelectedMenuItemChanged;

            OnHostControlAttached();
        }
    }

    private void SidebarMenu_OnSelectedMenuItemChanged(object? sender, RoutedEventArgs e)
    {
        if (SidebarMenu == null || NavigationStack == null)
            return;

        if (SidebarMenu.SelectedMenuItem != null)
            Navigation.PushAsync(NavigationStack.BuildRoute(SidebarMenu.SelectedMenuItem.RouteUri), SidebarMenu.SelectedMenuItem.Target);
    }

    protected override void UpdateContentSafeAreaPadding()
    {
        if (SidebarMenu != null)
            SidebarMenu.SafeAreaPadding = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);
    }
}