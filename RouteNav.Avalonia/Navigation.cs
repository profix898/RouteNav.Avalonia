using System;
using System.Threading.Tasks;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Platform;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia;

public static class Navigation
{
    public const string MainStackName = "main";

    public static Uri BaseRouteUri { get; set; } = new Uri("https://avalonia.local/");

    #region UIPlatform

    private static IUIPlatform? uiPlatform;

    public static IUIPlatform UIPlatform
    {
        get
        {
            return uiPlatform
                   ?? throw new NavigationException($"Implementation of {nameof(IUIPlatform)} is not available. Bootstrap via {nameof(AppBuilderExtensions.UseRouteNavUIPlatform)}().");
        }
        set { uiPlatform = value; }
    }

    #endregion

    #region Stacks

    public static INavigationStack? GetStack(string stackName)
    {
        return UIPlatform.GetStack(stackName);
    }

    public static INavigationStack GetMainStack() => UIPlatform.GetStack(MainStackName)
                                                     ?? throw new NavigationException("Main NavigationStack not available.");

    public static async Task<Page> EnterStack(INavigationStack? stack = null)
    {
        var activeStack = UIPlatform.ActivateStack(stack?.Name ?? MainStackName)
                          ?? throw new NavigationException("No NavigationStack available.");

        return await activeStack.PushAsync(activeStack.BaseUri);
    }

    #endregion

    #region RouteNavigation

    public static Task<Page> PushAsync(string stackName, string relativeRoute, NavigationTarget target = NavigationTarget.Self)
    {
        return PushAsync(BuildRoute(stackName, relativeRoute), target);
    }

    public static Task<Page> PushAsync(Uri routeUri, NavigationTarget target = NavigationTarget.Self)
    {
        switch (target)
        {
            case NavigationTarget.Self:
                // Open in current navigation container (potentially replacing current stack), with the exception of flyout-detail page
                // where we want to switch details content only (but only if flyout menu contains reference to requested stack root)
                return PushSelfAsync(routeUri);
            case NavigationTarget.Parent:
                // Open in parent container (replacing current stack or container)
                return PushParentAsync(routeUri);
            case NavigationTarget.Dialog:
                // Open in modal dialog (prefer in associated window)
                return PushDialogAsync(routeUri);
            case NavigationTarget.Window:
                // Open in new window (or replacing current stack if unsupported)
                return PushWindowAsync(routeUri);
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }

    public static Task PopAsync(Window? window = null)
    {
        return PopAsync(UIPlatform.GetActiveStackFromWindow(window));
    }

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

    public static bool PopAvailable(Window? window = null)
    {
        return PopAvailable(UIPlatform.GetActiveStackFromWindow(window));
    }

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

        // Check whether routeUri resolved to a valid stack
        if (!activeStack.BaseUri.IsBaseOf(routeUri))
        {
            var page404 = new NotFoundPage();
            await activeStack.PushAsync(page404);

            return page404;
        }

        return await activeStack.PushAsync(routeUri, NavigationTarget.Parent);
    }

    private static async Task<Page> PushDialogAsync(Uri routeUri)
    {
        // Open in modal dialog (prefer in associated window)
        var stackName = routeUri.GetStackName() ?? String.Empty;
        var activeStack = UIPlatform.GetActiveStack(stackName)
                          ?? UIPlatform.GetMainStack()
                          ?? throw new NavigationException("No NavigationStack available.");

        // Open in parent container (replacing current stack or container)
        if (!String.IsNullOrEmpty(stackName) && !stackName.Equals(activeStack.Name))
            activeStack = UIPlatform.ActivateStack(stackName, activeStack) ?? activeStack;

        // Check whether routeUri resolved to a valid stack
        if (!activeStack.BaseUri.IsBaseOf(routeUri))
        {
            var page404 = new NotFoundPage();
            await page404.ToDialog(activeStack.CurrentPage).ShowDialog();

            return page404;
        }

        return await activeStack.PushAsync(routeUri, NavigationTarget.Dialog);
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
        var stack = UIPlatform.ActivateStackWindow(stackName);
        if (stack != null)
            return await stack.PushAsync(routeUri);

        // Fallback: Replace page/stack in current window
        return await PushParentAsync(routeUri);
    }
    
    #endregion

    #endregion

    #region BuildRoute

    public static Uri BuildRoute(string stackName, string relativeRoute)
    {
        return new Uri(new Uri(BaseRouteUri, stackName + "/"), relativeRoute);
    }

    public static Uri BuildRoute(string stackName, Uri relativeRoute)
    {
        return new Uri(new Uri(BaseRouteUri, stackName + "/"), relativeRoute);
    }

    #endregion
}