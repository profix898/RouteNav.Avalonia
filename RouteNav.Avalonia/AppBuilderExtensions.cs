using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia.Platform;

namespace RouteNav.Avalonia;

public static class AppBuilderExtensions
{
    public static AppBuilder UseRouteNavUIPlatform<TContainer>(this AppBuilder builder, string baseRouteUri, TContainer container)
        where TContainer : IServiceCollection, IServiceProvider
    {
        return builder.UseRouteNavUIPlatform(baseRouteUri, container, container);
    }

    public static AppBuilder UseRouteNavUIPlatform(this AppBuilder builder, string baseRouteUri,
                                                   IServiceCollection serviceCollection, IServiceProvider serviceProvider)
    {
        if (!String.IsNullOrEmpty(baseRouteUri))
            Navigation.BaseRouteUri = new Uri(baseRouteUri);

        Navigation.UIPlatform = new AvaloniaUIPlatform(serviceCollection, serviceProvider);

        return builder;
    }

    public static AppBuilder UseRouteNavUIPlatform(this AppBuilder builder, string baseRouteUri, IUIPlatform uiPlatform)
    {
        if (!String.IsNullOrEmpty(baseRouteUri))
            Navigation.BaseRouteUri = new Uri(baseRouteUri);

        Navigation.UIPlatform = uiPlatform;

        return builder;
    }
}
