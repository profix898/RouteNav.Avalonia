using Android.App;
using Android.Content.PM;
using Android.Runtime;
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
public class MainActivity : AvaloniaMainActivity
{
}

// Avalonia 12 initializes the Android application (and its activities) through an
// AvaloniaAndroidApplication<TApp>. App builder customization (previously done on the activity)
// now lives here.
[Application]
public class AndroidApp : AvaloniaAndroidApplication<App>
{
    protected AndroidApp(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var serviceCollection = new ServiceCollection();

        return base.CustomizeAppBuilder(builder)
                   .UseRouteNavUIPlatform("https://test.local", serviceCollection.BuildServiceProvider, serviceCollection)
                   .LogToTrace();
    }
}
