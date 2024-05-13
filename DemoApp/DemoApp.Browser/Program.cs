using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using DemoApp;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia;

[assembly: SupportedOSPlatform("browser")]

internal class Program
{
    private static async Task Main(string[] args)
    {
        await BuildAvaloniaApp()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var serviceCollection = new ServiceCollection();

        return AppBuilder.Configure<App>()
                         .UseRouteNavUIPlatform("https://test.local", serviceCollection.BuildServiceProvider, serviceCollection);
    }
}
