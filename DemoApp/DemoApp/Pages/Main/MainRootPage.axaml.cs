using System;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Styling;
using RouteNav.Avalonia;

namespace DemoApp.Pages.Main;

public partial class MainRootPage : Page
{
    public MainRootPage()
    {
        InitializeComponent();
    }

    private void OnRouteGo(object? sender, RoutedEventArgs e)
    {
        var path = RouteInput.Text?.Trim();
        if (String.IsNullOrEmpty(path))
            return;

        var uri = new Uri(Navigation.BaseRouteUri, path.TrimStart('/'));
        _ = Navigation.PushAsync(uri);
    }

    private void OnSendParams(object? sender, RoutedEventArgs e)
    {
        var name = String.IsNullOrWhiteSpace(NameInput.Text) ? "Ada" : NameInput.Text!.Trim();
        var count = String.IsNullOrWhiteSpace(CountInput.Text) ? "3" : CountInput.Text!.Trim();

        var uri = new Uri(Navigation.BaseRouteUri, $"main/content?name={Uri.EscapeDataString(name)}&count={Uri.EscapeDataString(count)}");
        _ = Navigation.PushAsync(uri);
    }

    private void OnThemeLight(object? sender, RoutedEventArgs e) => SetTheme(ThemeVariant.Light);

    private void OnThemeDark(object? sender, RoutedEventArgs e) => SetTheme(ThemeVariant.Dark);

    private void OnThemeSystem(object? sender, RoutedEventArgs e) => SetTheme(ThemeVariant.Default);

    private static void SetTheme(ThemeVariant variant)
    {
        if (Application.Current != null)
            Application.Current.RequestedThemeVariant = variant;
    }
}
