using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace RouteNav.Avalonia;

/// <summary>
/// Provides functionality to launch hyperlinks within the application. It can handle both internal (routes) and external URIs.
/// </summary>
/// <remarks>
/// The class provides a RouteNav-compatible replacement for <see cref="ILauncher"/>.
/// </remarks>
public class HyperlinkLauncher : ILauncher
{
    #region Implementation of ILauncher

    public async Task<bool> LaunchUriAsync(Uri uri)
    {
        if (Navigation.BaseRouteUri.IsBaseOf(uri))
        {
            await Navigation.PushAsync(uri);
            return true;
        }

        return await AppUtility.GetTopLevel().Launcher.LaunchUriAsync(uri);
    }

    public async Task<bool> LaunchFileAsync(IStorageItem storageItem)
    {
        return await AppUtility.GetTopLevel().Launcher.LaunchFileAsync(storageItem);
    }

    #endregion
}
