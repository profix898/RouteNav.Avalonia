using System;
using Avalonia.Platform.Storage;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Platform;

/// <summary>
/// Abstraction of the UI platform that backs RouteNav: page registration/resolution, stack management
/// and stack activation into windows or views.
/// </summary>
public interface IUIPlatform
{
    /// <summary>Gets the window manager used to create and open platform windows and dialogs.</summary>
    public IWindowManager WindowManager { get; }

    /// <summary>Gets or sets the launcher used to open external URIs and files.</summary>
    public ILauncher Launcher { get; set; }

    #region Pages

    /// <summary>Registers one or more page types with the underlying DI container.</summary>
    void RegisterPage(params Type[] pageTypes);

    /// <summary>Resolves and instantiates a page of the given type for the given route URI.</summary>
    Page GetPage(Type pageType, Uri uri, params object[] parameters);

    #endregion

    #region Stacks

    /// <summary>Registers a navigation stack.</summary>
    void AddStack(INavigationStack stack);

    /// <summary>Removes a registered navigation stack by name.</summary>
    void RemoveStack(string stackName);

    /// <summary>Gets a registered navigation stack by name, or <c>null</c> if none is registered.</summary>
    INavigationStack? GetStack(string stackName);

    #endregion

    #region ActiveStacks

    /// <summary>Gets the main navigation stack.</summary>
    INavigationStack GetMainStack();

    /// <summary>Gets the currently active stack with the given name, or <c>null</c> if it is not active.</summary>
    INavigationStack? GetActiveStack(string stackName);

    /// <summary>Gets the active stack associated with the given window (or the main window when <c>null</c>).</summary>
    INavigationStack? GetActiveStackFromWindow(Window? window);

    /// <summary>Gets the window that hosts the given active stack, or <c>null</c> if it is not hosted.</summary>
    Window? GetActiveWindowFromStack(INavigationStack? navigationStack);

    #endregion

    #region Targets

    /// <summary>Activates the stack with the given name in the current (or source) window.</summary>
    INavigationStack? ActivateStack(string stackName, INavigationStack? sourceStack = null);

    /// <summary>Activates the stack with the given name in a new window (where supported).</summary>
    INavigationStack? ActivateStackInWindow(string stackName);

    #endregion
}