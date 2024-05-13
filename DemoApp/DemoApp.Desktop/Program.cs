using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia;

namespace DemoApp.Desktop;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var serviceCollection = new ServiceCollection();

        return AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .UseRouteNavUIPlatform("https://test.local", serviceCollection.BuildServiceProvider, serviceCollection)
                         .LogToTrace();
    }
}
