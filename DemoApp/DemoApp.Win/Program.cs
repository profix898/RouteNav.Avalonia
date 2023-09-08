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

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        return AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .UseRouteNavUIPlatform("http://test.ui", serviceCollection, serviceProvider)
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
