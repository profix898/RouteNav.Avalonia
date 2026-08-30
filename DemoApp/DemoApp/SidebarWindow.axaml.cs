using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RouteNav.Avalonia;
using Window = RouteNav.Avalonia.Window;

namespace DemoApp;

/// <summary>
/// Custom window shell for the sidebar navigation stack: shows a toolbar with quick links
/// above the navigation surface. RouteNav routes stack content into <see cref="ContentHost" />
/// through the <c>SetContentCore</c> override.
/// </summary>
public partial class SidebarWindow : Window
{
    public SidebarWindow()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void SetContentCore(Control content)
    {
        ContentHost.Content = content;
    }

    private void OnBack(object? sender, RoutedEventArgs e)
    {
        _ = Navigation.PopAsync(this);
    }
}
