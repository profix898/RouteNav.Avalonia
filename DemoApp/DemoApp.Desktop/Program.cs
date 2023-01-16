using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Microsoft.Extensions.DependencyInjection;
using NSE.RouteNav.Bootstrap;

namespace DemoApp.Desktop
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseRouteNavPlatform("https://myapp.local")
                .UseServiceCollection()
                .LogToTrace()
                .UseReactiveUI();
    }

    public static class AppBuilderExtensions
    {
        public static AppBuilder UseServiceCollection(this AppBuilder builder)
        {
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            AvaloniaLocator.CurrentMutable.Bind<IServiceCollection>().ToConstant(serviceCollection);
            AvaloniaLocator.CurrentMutable.Bind<IServiceProvider>().ToConstant(serviceProvider);

            return builder;
        }
    }
}