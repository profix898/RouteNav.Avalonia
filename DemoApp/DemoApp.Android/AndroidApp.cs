using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia;

namespace DemoApp.Android;

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
