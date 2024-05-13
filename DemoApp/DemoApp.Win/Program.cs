using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia;

namespace DemoApp.Win;

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
                         .AfterSetup(_ =>
                         {
                             Navigation.UIPlatform.WindowManager.WindowCustomizationEvent += (window, isDialogWindow) =>
                             {
                                 if (isDialogWindow)
                                     window.SetDialogModalFrame();
                             };
                         })
                         .LogToTrace();
    }
}
