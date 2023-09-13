using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia;

namespace DemoApp.Android;

[Activity(
    Label = "DemoApp.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var serviceCollection = new ServiceCollection();

        return base.CustomizeAppBuilder(builder)
                   .UseRouteNavUIPlatform("http://test.ui", serviceCollection.BuildServiceProvider, serviceCollection)
                   .LogToTrace();
    }
}
