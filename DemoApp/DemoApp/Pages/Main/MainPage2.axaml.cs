using Avalonia.Interactivity;
using RouteNav.Avalonia;

namespace DemoApp.Pages.Main;

public partial class MainPage2 : Page
{
    public MainPage2()
    {
        InitializeComponent();
    }

    private void PushPageCommand(object? sender, RoutedEventArgs e)
    {
        var stack = Navigation.GetMainStack(); // or Navigation.GetStack("myStack")
        
        stack.PushAsync(new TestPage());
    }

    private async void PushDialogCommand(object? sender, RoutedEventArgs e)
    {
        var stack = Navigation.GetMainStack(); // or Navigation.GetStack("myStack")
        
        await stack.PushDialogAsync(new TestDialog());
        await stack.PushDialogAsync(new TestDialog(), true); // Overlay dialog
    }
}
