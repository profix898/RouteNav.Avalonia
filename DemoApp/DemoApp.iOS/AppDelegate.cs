using Avalonia;
using Avalonia.iOS;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia;

namespace DemoApp.iOS;

[Register("AppDelegate")]
public class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var serviceCollection = new ServiceCollection();

        return base.CustomizeAppBuilder(builder)
                   .UseRouteNavUIPlatform("https://test.local", serviceCollection.BuildServiceProvider, serviceCollection)
                   .LogToTrace();
    }
}
