using System;
using Avalonia;
using NSE.RouteNav.Platform;

namespace NSE.RouteNav.Bootstrap;

public static class AppBuilderExtensions
{
    public static AppBuilder UseRouteNavPlatform(this AppBuilder builder, string baseRouteUri)
    {
        if (!String.IsNullOrEmpty(baseRouteUri))
            Navigation.BaseRouteUri = new Uri(baseRouteUri);

        AvaloniaLocator.CurrentMutable.Bind<IUIPlatform>().ToSingleton<AvaloniaUIPlatform>();

        return builder;
    }
}
