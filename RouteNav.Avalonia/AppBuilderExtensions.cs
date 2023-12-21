using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia.Platform;

namespace RouteNav.Avalonia;

public static class AppBuilderExtensions
{
    /// <summary>For use with a single/simple (non-locking) DI container (i.e. dictionary).</summary>
    public static AppBuilder UseRouteNavUIPlatform<TContainer>(this AppBuilder builder, string baseRouteUri, TContainer container)
        where TContainer : IServiceCollection, IServiceProvider
    {
        return UseRouteNavUIPlatform(builder, baseRouteUri, new Lazy<IServiceProvider>(container), container);
    }

    /// <summary>Uses an IServiceCollection (for page registration) and an IServiceProvider factory (to fetch components afterwards).</summary>
    /// <remarks>IServiceCollection is only ever used for 'RegisterPage' calls during initialization.
    ///          You can also register any pages (and dependencies) directly with the DI container.</remarks>
    public static AppBuilder UseRouteNavUIPlatform(this AppBuilder builder, string baseRouteUri,
                                                   Func<IServiceProvider> serviceProvider, IServiceCollection? serviceCollection = null)
    {
        return UseRouteNavUIPlatform(builder, baseRouteUri, new Lazy<IServiceProvider>(serviceProvider), serviceCollection);
    }

    /// <summary>Uses an IServiceCollection (for page registration) and an IServiceProvider factory (to fetch components afterwards).</summary>
    /// <remarks>IServiceCollection is only ever used for 'RegisterPage' calls during initialization.
    ///          You can also register any pages (and dependencies) directly with the DI container.</remarks>
    public static AppBuilder UseRouteNavUIPlatform(this AppBuilder builder, string baseRouteUri,
                                                   Lazy<IServiceProvider> serviceProvider, IServiceCollection? serviceCollection = null)
    {
        if (!String.IsNullOrEmpty(baseRouteUri))
            Navigation.BaseRouteUri = new Uri(baseRouteUri);

        Navigation.UIPlatform = new AvaloniaUIPlatform(serviceProvider, serviceCollection);

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
