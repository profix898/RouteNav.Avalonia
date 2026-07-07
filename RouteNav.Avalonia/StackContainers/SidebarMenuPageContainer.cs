using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.StackContainers;

/// <summary>A navigation container that hosts pages in a <see cref="SidebarMenu"/>.</summary>
public class SidebarMenuPageContainer : NavigationContainer
{
    /// <summary>Defines the <see cref="SidebarMenuName"/> property.</summary>
    public static readonly StyledProperty<string> SidebarMenuNameProperty = AvaloniaProperty.Register<SidebarMenuPageContainer, string>(nameof(SidebarMenuName), "SidebarMenu");

    /// <summary>Initializes a new instance of the <see cref="SidebarMenuPageContainer"/> class.</summary>
    public SidebarMenuPageContainer()
    {
        RegisterScopedControl(this, SidebarMenuName, Content = new SidebarMenu());
    }

    /// <summary>Gets or sets the scoped name used to resolve the sidebar menu.</summary>
    public string SidebarMenuName
    {
        get { return GetValue(SidebarMenuNameProperty); }
        set { SetValue(SidebarMenuNameProperty, value); }
    }

    /// <summary>Gets the resolved sidebar menu.</summary>
    public SidebarMenu? SidebarMenu { get; private set; }

    /// <inheritdoc />
    public override void UpdatePage(Page page)
    {
        if (SidebarMenu != null && NavigationStack != null)
            SidebarMenu.Page = page;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    protected override void UpdateContentSafeAreaPadding()
    {
        if (SidebarMenu != null)
            SidebarMenu.SafeAreaPadding = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);
    }
}