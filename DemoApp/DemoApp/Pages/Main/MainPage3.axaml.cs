using Avalonia.Interactivity;
using RouteNav.Avalonia;

namespace DemoApp.Pages.Main;

public partial class MainPage3 : Page
{
    public MainPage3()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (PageQuery.TryGetValue("name", out var name))
            ParamName.Text = name;
        if (PageQuery.TryGetValue("count", out var count))
            ParamCount.Text = count;
        if (PageQuery.TryGetValue("routeUri", out var routeUri))
            ParamRaw.Text = routeUri;
    }
}
