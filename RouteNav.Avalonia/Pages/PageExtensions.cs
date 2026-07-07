using System;
using RouteNav.Avalonia.Platform;

namespace RouteNav.Avalonia.Pages;

/// <summary>Convenience extensions for registering and resolving pages on an <see cref="IUIPlatform" />.</summary>
public static class PageExtensions
{
    #region Register

    /// <summary>Registers the given page type with the DI container.</summary>
    public static void RegisterPage<T1>(this IUIPlatform uiPlatform)
        where T1 : Page
    {
        uiPlatform.RegisterPage(typeof(T1));
    }

    /// <summary>Registers the given page types with the DI container.</summary>
    public static void RegisterPage<T1, T2>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2));
    }

    /// <summary>Registers the given page types with the DI container.</summary>
    public static void RegisterPage<T1, T2, T3>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
        where T3 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2), typeof(T3));
    }

    /// <summary>Registers the given page types with the DI container.</summary>
    public static void RegisterPage<T1, T2, T3, T4>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
        where T3 : Page
        where T4 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }

    /// <summary>Registers the given page types with the DI container.</summary>
    public static void RegisterPage<T1, T2, T3, T4, T5>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
        where T3 : Page
        where T4 : Page
        where T5 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }

    /// <summary>Registers the given page types with the DI container.</summary>
    public static void RegisterPage<T1, T2, T3, T4, T5, T6>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
        where T3 : Page
        where T4 : Page
        where T5 : Page
        where T6 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }

    /// <summary>Registers the given page types with the DI container.</summary>
    public static void RegisterPage<T1, T2, T3, T4, T5, T6, T7>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
        where T3 : Page
        where T4 : Page
        where T5 : Page
        where T6 : Page
        where T7 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
    }

    /// <summary>Registers the given page types with the DI container.</summary>
    public static void RegisterPage<T1, T2, T3, T4, T5, T6, T7, T8>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
        where T3 : Page
        where T4 : Page
        where T5 : Page
        where T6 : Page
        where T7 : Page
        where T8 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
    }

    /// <summary>Registers the given page types with the DI container.</summary>
    public static void RegisterPage<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
        where T3 : Page
        where T4 : Page
        where T5 : Page
        where T6 : Page
        where T7 : Page
        where T8 : Page
        where T9 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
    }

    #endregion

    #region Get

    /// <summary>Resolves a page of type <typeparamref name="T1" /> using the base route URI.</summary>
    public static Page GetPage<T1>(this IUIPlatform uiPlatform, params object[] parameters)
        where T1 : Page
    {
        return uiPlatform.GetPage(typeof(T1), Navigation.BaseRouteUri, parameters);
    }

    /// <summary>Resolves a page of type <typeparamref name="T1" /> for the given route URI.</summary>
    public static Page GetPage<T1>(this IUIPlatform uiPlatform, Uri uri, params object[] parameters)
        where T1 : Page
    {
        return uiPlatform.GetPage(typeof(T1), uri, parameters);
    }

    #endregion
}
