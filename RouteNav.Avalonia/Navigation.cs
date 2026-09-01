using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Platform;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia;

/// <summary>Static facade for RouteNav stack activation, route navigation and pop operations.</summary>
public static class Navigation
{
    /// <summary>The default name of the main navigation stack.</summary>
    public const string DefaultMainStackName = "main";

    #region Options

    private static string mainStackName = DefaultMainStackName;
    private static WindowOptions windows = new();
    private static DialogOptions dialogs = new();

    /// <summary>
    /// Gets or sets the name of the main navigation stack (the stack entered at application startup and
    /// the fallback target when a route has no stack name). Set this at application startup, before any
    /// navigation stacks are registered or activated.
    /// </summary>
    public static string MainStackName
    {
        get => mainStackName;
        set
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("The main stack name cannot be null or empty.", nameof(value));

            if (uiPlatform is { RegisteredStacks.Count: > 0 })
                throw new NavigationException("The main stack name cannot be changed after navigation stacks are registered.");

            mainStackName = value;
        }
    }

    /// <summary>Gets or sets the base URI that all stack and page routes are resolved against.</summary>
    public static Uri BaseRouteUri { get; set; } = new Uri("https://avalonia.local/");

    /// <summary>Gets or sets the window-manager options (single-window mode, overlay dialogs, window activation).</summary>
    public static WindowOptions Windows
    {
        get => windows;
        set => windows = value ?? throw new ArgumentNullException(nameof(value), "Window options cannot be null.");
    }

    /// <summary>Gets or sets the dialog options (sizing defaults and overlay animation).</summary>
    public static DialogOptions Dialogs
    {
        get => dialogs;
        set => dialogs = value ?? throw new ArgumentNullException(nameof(value), "Dialog options cannot be null.");
    }

    #endregion

    #region UIPlatform

    private static IUIPlatform? uiPlatform;

    /// <summary>
    /// Gets or sets the active <see cref="IUIPlatform" /> implementation that backs all navigation.
    /// </summary>
    /// <exception cref="NavigationException">Thrown when accessed before a UI platform has been bootstrapped.</exception>
    public static IUIPlatform UIPlatform
    {
        get { return uiPlatform ?? throw new NavigationException($"Implementation of {nameof(IUIPlatform)} is not available. Bootstrap via {nameof(AppBuilderExtensions.UseRouteNavUIPlatform)}()."); }
        set { uiPlatform = value; }
    }

    #endregion

    #region Stacks

    /// <summary>Gets the registered navigation stack with the given name, or <c>null</c> if none is registered.</summary>
    public static INavigationStack? GetStack(string stackName)
    {
        return UIPlatform.GetStack(stackName);
    }

    /// <summary>Gets the main navigation stack.</summary>
    /// <exception cref="NavigationException">Thrown when the main stack is not available.</exception>
    public static INavigationStack GetMainStack()
        => UIPlatform.GetStack(MainStackName)
           ?? throw new NavigationException("Main NavigationStack not available.");

    /// <summary>Activates the given stack (or the main stack) and pushes its root page.</summary>
    /// <param name="stack">The stack to enter, or <c>null</c> to enter the main stack.</param>
    public static async Task<Page> EnterStack(INavigationStack? stack = null)
    {
        var activeStack = UIPlatform.ActivateStack(stack?.Name ?? MainStackName)
                          ?? throw new NavigationException("No NavigationStack available.");

        return await activeStack.PushAsync(activeStack.BuildRoute(String.Empty));
    }

    /// <summary>Gets all registered navigation stacks (regardless of active state), for introspection.</summary>
    public static IReadOnlyList<INavigationStack> RegisteredStacks => UIPlatform.RegisteredStacks;

    /// <summary>Gets the navigation stacks that are currently hosted in an open window (in hosting order).</summary>
    public static IReadOnlyList<INavigationStack> ActiveStacks => UIPlatform.ActiveStacks;

    #endregion

    #region RouteNavigation

    /// <summary>Navigates to a route built from a stack name and a relative route.</summary>
    /// <param name="stackName">The target stack name.</param>
    /// <param name="relativeRoute">The route relative to the stack root.</param>
    /// <param name="target">Where the resolved page should be shown.</param>
    public static Task<Page> PushAsync(string stackName, string relativeRoute, NavigationTarget target = NavigationTarget.Self)
    {
        return PushAsync(BuildRoute(stackName, relativeRoute), target);
    }

    /// <summary>Navigates to the page identified by <paramref name="routeUri" />.</summary>
    /// <param name="routeUri">The (absolute or relative) route URI to resolve.</param>
    /// <param name="target">Where the resolved page should be shown.</param>
    public static Task<Page> PushAsync(Uri routeUri, NavigationTarget target = NavigationTarget.Self)
    {
        return target switch
        {
            // Open in current navigation container (potentially replacing current stack), with the exception of flyout-detail page
            // where we want to switch details content only (but only if flyout menu contains reference to requested stack root)
            NavigationTarget.Self => PushSelfAsync(routeUri),

            // Open in parent container (replacing current stack or container)
            NavigationTarget.Parent => PushParentAsync(routeUri),

            // Open in modal dialog (prefer in associated window)
            NavigationTarget.Dialog => PushDialogAsync(routeUri),
            NavigationTarget.DialogOverlay => PushDialogAsync(routeUri, target),

            // Open in new window (or replacing current stack if unsupported)
            NavigationTarget.Window => PushWindowAsync(routeUri),

            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }

    /// <summary>Pops the current page/dialog from the stack associated with the given window.</summary>
    /// <param name="window">The window whose active stack to pop, or <c>null</c> for the main window.</param>
    public static Task PopAsync(Window? window = null)
    {
        return PopAsync(UIPlatform.GetActiveStackFromWindow(window));
    }

    /// <summary>Pops the current page/dialog from the given stack (dialogs first, then pages).</summary>
    /// <param name="stack">The stack to pop, or <c>null</c> for the main stack.</param>
    public static Task PopAsync(INavigationStack? stack = null)
    {
        var activeStack = stack
                          ?? UIPlatform.GetMainStack()
                          ?? throw new NavigationException("No NavigationStack available.");

        if (activeStack.DialogStack.Count > 0)
            return PopDialogAsync(activeStack); // There are pages on the dialog stack -> pop those first

        if (activeStack.PageStack.Count > 0)
            return activeStack.PopAsync();

        if (!activeStack.IsMainStack)
        {
            // Empty (extra) stack -> switch to main stack
            var mainStack = UIPlatform.ActivateStack(MainStackName, activeStack)
                            ?? throw new NavigationException("Main NavigationStack not available.");

            return mainStack.PushAsync(mainStack.BaseUri);
        }

        return Task.FromResult(activeStack.CurrentPage);
    }

    /// <summary>Gets a value indicating whether a pop operation is currently available for the given window's stack.</summary>
    public static bool PopAvailable(Window? window = null)
    {
        return PopAvailable(UIPlatform.GetActiveStackFromWindow(window));
    }

    /// <summary>Gets a value indicating whether a pop operation is currently available for the given stack.</summary>
    public static bool PopAvailable(INavigationStack? stack = null)
    {
        var activeStack = stack
                          ?? UIPlatform.GetMainStack()
                          ?? throw new NavigationException("No NavigationStack available.");

        if (activeStack.DialogStack.Count > 0)
            return true;

        if (activeStack.PageStack.Count > (activeStack.IsMainStack ? 1 : 0))
            return true;

        if (!activeStack.IsMainStack)
            return true;

        return false;
    }

    #region Internal

    private static async Task<Page> PushSelfAsync(Uri routeUri)
    {
        var stackName = routeUri.GetStackName() ?? String.Empty;
        var activeStack = UIPlatform.GetActiveStack(stackName)
                          ?? UIPlatform.GetMainStack()
                          ?? throw new NavigationException("No NavigationStack available.");

        // Open in current navigation container (potentially replacing current stack), with the exception of flyout-detail page
        // where we want to switch details content only (but only if flyout menu contains reference to requested stack root)
        if (!String.IsNullOrEmpty(stackName) && !stackName.Equals(activeStack.Name))
        {
            activeStack = activeStack.RequestStack(stackName)
                          ?? UIPlatform.ActivateStack(stackName, activeStack)
                          ?? activeStack;
        }

        // Ensure that we can always return to the main stack
        if (activeStack.IsMainStack && UIPlatform.GetActiveStack(MainStackName) == null)
            activeStack = UIPlatform.ActivateStack(stackName);

        // Bring the window hosting the target stack to the foreground (if navigation targets another window)
        UIPlatform.BringStackWindowToFront(activeStack);

        // Check whether routeUri resolved to a valid stack
        if (!activeStack.BaseUri.IsBaseOf(routeUri))
        {
            var page404 = new NotFoundPage();
            await activeStack.PushAsync(page404);

            return page404;
        }

        return await activeStack.PushAsync(routeUri);
    }

    private static async Task<Page> PushParentAsync(Uri routeUri)
    {
        var stackName = routeUri.GetStackName() ?? String.Empty;
        var activeStack = UIPlatform.GetActiveStack(stackName)
                          ?? UIPlatform.GetMainStack()
                          ?? throw new NavigationException("No NavigationStack available.");

        // Open in parent container (replacing current stack or container)
        if (!String.IsNullOrEmpty(stackName) && !stackName.Equals(activeStack.Name))
            activeStack = UIPlatform.ActivateStack(stackName, activeStack) ?? activeStack;

        // Ensure that we can always return to the main stack
        if (activeStack.IsMainStack && UIPlatform.GetActiveStack(MainStackName) == null)
            activeStack = UIPlatform.ActivateStack(stackName);

        // Bring the window hosting the target stack to the foreground (if navigation targets another window)
        UIPlatform.BringStackWindowToFront(activeStack);

        // Check whether routeUri resolved to a valid stack
        if (!activeStack.BaseUri.IsBaseOf(routeUri))
        {
            var page404 = new NotFoundPage();
            await activeStack.PushAsync(page404);

            return page404;
        }

        return await activeStack.PushAsync(routeUri, NavigationTarget.Parent);
    }

    private static async Task<Page> PushDialogAsync(Uri routeUri, NavigationTarget target = NavigationTarget.Dialog)
    {
        // Open in modal dialog (prefer in associated window)
        var stackName = routeUri.GetStackName() ?? String.Empty;
        var activeStack = UIPlatform.GetActiveStack(stackName)
                          ?? UIPlatform.GetMainStack()
                          ?? throw new NavigationException("No NavigationStack available.");

        // Open in parent container (replacing current stack or container)
        if (!String.IsNullOrEmpty(stackName) && !stackName.Equals(activeStack.Name))
            activeStack = UIPlatform.ActivateStack(stackName, activeStack) ?? activeStack;

        // Ensure that we can always return to the main stack
        if (activeStack.IsMainStack && UIPlatform.GetActiveStack(MainStackName) == null)
            activeStack = UIPlatform.ActivateStack(stackName);

        // Bring the window hosting the target stack to the foreground (if navigation targets another window)
        UIPlatform.BringStackWindowToFront(activeStack);

        // Check whether routeUri resolved to a valid stack
        if (!activeStack.BaseUri.IsBaseOf(routeUri))
        {
            var page404 = new NotFoundPage();
            await activeStack.PushDialogAsync(page404, forceOverlay: target == NavigationTarget.DialogOverlay);

            return page404;
        }

        return await activeStack.PushAsync(routeUri, target);
    }

    private static Task PopDialogAsync(INavigationStack? stack = null)
    {
        var activeStack = stack
                          ?? UIPlatform.GetMainStack()
                          ?? throw new NavigationException("No NavigationStack available.");

        return activeStack.PopDialogAsync();
    }

    private static async Task<Page> PushWindowAsync(Uri routeUri)
    {
        // Open in new window (if supported)
        var stackName = routeUri.GetStackName() ?? String.Empty;
        var stack = UIPlatform.ActivateStackInWindow(stackName);
        if (stack != null)
            return await stack.PushAsync(routeUri);

        // Fallback: Replace page/stack in current window
        return await PushParentAsync(routeUri);
    }

    #endregion

    #endregion

    #region BuildRoute

    /// <summary>Builds an absolute route URI from a stack name and a relative route.</summary>
    public static Uri BuildRoute(string stackName, string relativeRoute)
    {
        // Root-relative route ('/path'): resolve against the host root (legacy semantics)
        if (relativeRoute.StartsWith('/'))
            return new Uri(new Uri(BaseRouteUri, stackName + "/"), relativeRoute);

        var baseUri = BaseRouteUri.AbsoluteUri;
        if (!baseUri.EndsWith('/'))
            baseUri += "/";

        var stack = stackName.AsSpan().Trim('/');
        return stack.IsEmpty
            ? new Uri(String.Concat(baseUri, relativeRoute))
            : new Uri(String.Concat(baseUri, stack, "/", relativeRoute));
    }

    /// <summary>Builds an absolute route URI from a stack name and a relative route.</summary>
    public static Uri BuildRoute(string stackName, Uri relativeRoute)
    {
        if (relativeRoute.IsAbsoluteUri)
            return relativeRoute;

        return BuildRoute(stackName, relativeRoute.OriginalString);
    }

    #endregion
}
