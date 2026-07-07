using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Page = RouteNav.Avalonia.Page;
using SidebarMenuControl = RouteNav.Avalonia.Controls.SidebarMenu;

namespace DemoApp.Pages.SidebarMenu;

public partial class SidebarMenuRootPage : Page
{
    public SidebarMenuRootPage()
    {
        InitializeComponent();
    }

    private void OnDisplayMode(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: string modeName })
            return;

        if (!Enum.TryParse<SidebarMenuControl.DisplayModeEnum>(modeName, out var mode))
            return;

        var sidebar = this.FindAncestorOfType<SidebarMenuControl>();
        if (sidebar != null)
            sidebar.DisplayMode = mode;
    }
}
