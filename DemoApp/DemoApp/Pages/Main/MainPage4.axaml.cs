using Avalonia.Controls;
using Avalonia.Interactivity;
using RouteNav.Avalonia;
using Page = RouteNav.Avalonia.Page;

namespace DemoApp.Pages.Main;

public partial class MainPage4 : Page
{
    public MainPage4()
    {
        InitializeComponent();
    }

    private void OnCloseWindow(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Avalonia.Controls.Window window)
            window.Close();
    }
}
