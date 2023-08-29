using RouteNav.Avalonia.Platform;
using System;

namespace RouteNav.Avalonia.Pages;

public static class PageExtensions
{
    #region Register

    public static void RegisterPage<T1>(this IUIPlatform uiPlatform)
        where T1 : Page
    {
        uiPlatform.RegisterPage(typeof(T1));
    }

    public static void RegisterPage<T1, T2>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2));
    }

    public static void RegisterPage<T1, T2, T3>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
        where T3 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2), typeof(T3));
    }

    public static void RegisterPage<T1, T2, T3, T4>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
        where T3 : Page
        where T4 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }

    public static void RegisterPage<T1, T2, T3, T4, T5>(this IUIPlatform uiPlatform)
        where T1 : Page
        where T2 : Page
        where T3 : Page
        where T4 : Page
        where T5 : Page
    {
        uiPlatform.RegisterPage(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }

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

    public static Page GetPage<T1>(this IUIPlatform uiPlatform, params object[] parameters)
        where T1 : Page
    {
        return uiPlatform.GetPage(typeof(T1), Navigation.BaseRouteUri, parameters);
    }

    public static Page GetPage<T1>(this IUIPlatform uiPlatform, Uri uri, params object[] parameters)
        where T1 : Page
    {
        return uiPlatform.GetPage(typeof(T1), uri, parameters);
    }

    #endregion
}
