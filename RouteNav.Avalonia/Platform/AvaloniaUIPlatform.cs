using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia.Errors;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.Stacks;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Platform;

/// <summary>
/// Default <see cref="IUIPlatform" /> implementation backed by Avalonia windows and views.
/// </summary>
/// <remarks>
/// Instances are not thread-safe. All navigation operations (stack registration, activation and
/// page/dialog pushes) are expected to run on the UI thread, which serializes access to the internal
/// stack dictionaries.
/// </remarks>
public class AvaloniaUIPlatform : IUIPlatform
{
    private readonly Dictionary<string, INavigationStack> navigationStacks = new Dictionary<string, INavigationStack>();
    private readonly Dictionary<Window, INavigationStack> activeStacks = new Dictionary<Window, INavigationStack>();

    private readonly Lazy<IServiceProvider> serviceProvider;
    private readonly IServiceCollection? serviceCollection;

    /// <summary>Initializes a new instance of the <see cref="AvaloniaUIPlatform" /> class with the default window manager.</summary>
    public AvaloniaUIPlatform(Lazy<IServiceProvider> serviceProvider, IServiceCollection? serviceCollection)
        : this(new AvaloniaWindowManager(), serviceProvider, serviceCollection)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="AvaloniaUIPlatform" /> class.</summary>
    public AvaloniaUIPlatform(IWindowManager windowManager, Lazy<IServiceProvider> serviceProvider, IServiceCollection? serviceCollection)
    {
        WindowManager = windowManager;
        Launcher = new HyperlinkLauncher();

        this.serviceProvider = serviceProvider;
        this.serviceCollection = serviceCollection;

        RegisterPage(typeof(NotFoundPage), typeof(InternalErrorPage));
    }

    #region IUIPlatform Members

    /// <inheritdoc />
    public IWindowManager WindowManager { get; }

    /// <inheritdoc />
    public WindowFactory? DefaultWindowFactory { get; set; } = _ => new Window();

    /// <inheritdoc />
    public ILauncher Launcher { get; set; }

    /// <inheritdoc />
    public Window CreateWindow(WindowCreationContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Dialog windows always use the application-wide default factory: custom stack window
        // chrome (toolbars, menus) must not wrap dialog content.
        var factory = (context.Kind == WindowKind.Dialog
                          ? DefaultWindowFactory
                          : context.Stack?.WindowFactory ?? DefaultWindowFactory)
                      ?? throw new NavigationException("No window factory is configured.");
        var window = factory(context)
                     ?? throw new NavigationException("The window factory returned null.");

        if (window.Parent != null || window.PlatformControl != null)
            throw new NavigationException("The window factory must return a fresh, unattached window instance.");

        // Lifecycle: when a window closes it no longer hosts its navigation stack
        window.Closed += (_, _) => activeStacks.Remove(window);

        if (context.Content is Control contentControl)
            window.SetInitialContent(contentControl);
        else if (context.Content != null)
            window.Content = context.Content;
        if (context.Title != null)
            window.Title = context.Title;
        if (context.Icon != null)
            window.Icon = context.Icon;

        return window;
    }

    /// <inheritdoc />
    public bool ReplaceActiveWindow(Window previousWindow, Window newWindow)
    {
        if (previousWindow == null)
            throw new ArgumentNullException(nameof(previousWindow));
        if (newWindow == null)
            throw new ArgumentNullException(nameof(newWindow));

        if (!activeStacks.Remove(previousWindow, out var stack))
            return false;

        previousWindow.Content = null;
        activeStacks.Add(newWindow, stack);
        newWindow.SetContent(stack.ContainerPage.Value);
        return true;
    }

    #region Pages

    /// <inheritdoc />
    public void RegisterPage(params Type[] pageTypes)
    {
        if (pageTypes == null)
            throw new ArgumentNullException(nameof(pageTypes));

        if (serviceCollection == null)
            throw new NotSupportedException("IServiceCollection is not available (for page registration).");

        foreach (var pageType in pageTypes)
        {
            if (!pageType.IsSubclassOf(typeof(Page)))
                throw new ArgumentException($"Type '{pageType.FullName}' is not a page.", nameof(pageTypes));

            serviceCollection.AddTransient(pageType, pageType);
        }
    }

    /// <inheritdoc />
    public Page GetPage(Type pageType, Uri uri, params object[] parameters)
    {
        if (pageType == null)
            throw new ArgumentNullException(nameof(pageType));

        if (serviceProvider.Value == null)
            throw new NotSupportedException($"IServiceProvider is not available (can't retrieve page of type '{pageType.FullName}').");

        try
        {
            var page = (Page) ActivatorUtilities.CreateInstance(serviceProvider.Value, pageType, parameters);

            // Supply page with query parameters (if available)
            page.PageQuery = uri.ParseQueryString();
            page.PageQuery.Add("routeUri", uri.ToString());

            return page;
        }
        catch (Exception ex)
        {
            return Error.Page($"Failed to retrieve page of type '{pageType.FullName}'.", ex);
        }
    }

    #endregion

    #region Stacks

    /// <inheritdoc />
    public void AddStack(INavigationStack stack)
    {
        if (stack == null)
            throw new ArgumentNullException(nameof(stack));

        navigationStacks.Add(stack.Name, stack);
    }

    /// <inheritdoc />
    public void RemoveStack(string stackName)
    {
        if (activeStacks.Any(stack => stack.Value.Name.Equals(stackName)))
            throw new ArgumentException($"Stack '{stackName}' is currently in use.", nameof(stackName));

        navigationStacks.Remove(stackName);
    }

    /// <inheritdoc />
    public INavigationStack? GetStack(string stackName)
    {
        return navigationStacks.TryGetValue(stackName, out var stack) ? stack : null;
    }

    #endregion

    #region ActiveStacks

    /// <inheritdoc />
    public INavigationStack GetMainStack()
    {
        if (navigationStacks.TryGetValue(Navigation.MainStackName, out var mainStack))
            return mainStack;

        return activeStacks.Values.FirstOrDefault()
               ?? throw new NavigationException($"Stack '{Navigation.MainStackName}' is not available.");
    }

    /// <inheritdoc />
    public INavigationStack? GetActiveStack(string stackName)
    {
        if (String.IsNullOrEmpty(stackName))
            return GetMainStack();

        return activeStacks.FirstOrDefault(stack => stack.Value.Name.Equals(stackName)).Value;
    }

    /// <inheritdoc />
    public INavigationStack? GetActiveStackFromWindow(Window? window)
    {
        window ??= Application.Current!.GetMainWindow();

        return activeStacks.TryGetValue(window, out var stack) ? stack : null;
    }

    /// <inheritdoc />
    public Window? GetActiveWindowFromStack(INavigationStack? navigationStack)
    {
        navigationStack ??= GetMainStack();

        if (activeStacks.ContainsValue(navigationStack))
            return activeStacks.First(stackWindow => stackWindow.Value == navigationStack).Key;

        return null;
    }

    /// <inheritdoc />
    public void BringStackWindowToFront(INavigationStack? stack)
    {
        if (stack == null)
            return;

        var window = GetActiveWindowFromStack(stack);
        if (window != null)
        {
            // Bring the window hosting the stack to the foreground
            if (!Navigation.Windows.BringTargetWindowToFront)
                return;

            if (window.PlatformControl is AvaloniaWindow platformWindow && !platformWindow.IsActive)
                window.Activate();
        }
        else if (stack.IsMainStack && Application.Current?.GetMainWindow() is { IsClosed: true } closedWindow)
        {
            // The main stack is unhosted because its window was closed -> re-open the main window
            ReopenMainWindow(closedWindow, stack);
        }
    }

    #endregion

    #region Targets

    /// <inheritdoc />
    public INavigationStack? ActivateStack(string stackName, INavigationStack? sourceStack = null)
    {
        if (String.IsNullOrEmpty(stackName))
            return null;

        var stack = GetStack(stackName);
        if (stack != null && !stack.IsEventStack)
        {
            var window = GetActiveWindowFromStack(stack);
            var sourceWindow = sourceStack != null ? GetActiveWindowFromStack(sourceStack) : null;
            if (window == null)
            {
                // The main stack is unhosted because its window was closed -> re-open the main window
                if (stack.IsMainStack && Application.Current?.GetMainWindow() is { IsClosed: true } closedWindow)
                {
                    ReopenMainWindow(closedWindow, stack);

                    // The source window hosted the (switched-away) stack and is no longer needed
                    if (sourceWindow != null)
                    {
                        activeStacks.Remove(sourceWindow);
                        sourceWindow.Close();
                        sourceStack?.Reset();
                    }

                    return stack ?? GetMainStack();
                }

                // Display in window associated with sourceStack or fall back to main/first application window
                sourceWindow ??= Application.Current?.GetMainWindow();
                if (sourceWindow == null || sourceWindow.IsClosed)
                    throw new NavigationException("No main window/view available. Application not fully initialized yet.");

                // Associate initial/main window with stack
                if (activeStacks.Count == 0)
                    activeStacks.Add(sourceWindow, stack);

                // Switch current/active stack
                activeStacks[sourceWindow] = stack;
                sourceWindow.SetContent(stack.ContainerPage.Value);
                sourceStack?.Reset();
            }
            else if (sourceWindow != null && window != sourceWindow)
            {
                // Close source window
                activeStacks.Remove(sourceWindow);
                sourceWindow.Close();
                sourceStack?.Reset();

                // Bring the window hosting the target stack to the foreground
                if (Navigation.Windows.BringTargetWindowToFront)
                    window.Activate();
            }
        }

        return stack ?? GetMainStack();
    }

    /// <inheritdoc />
    public INavigationStack? ActivateStackInWindow(string stackName)
    {
        if (String.IsNullOrEmpty(stackName))
            return null;

        var stack = GetStack(stackName);
        if (stack != null && !stack.IsEventStack)
        {
            var window = GetActiveWindowFromStack(stack);
            if (window == null)
            {
                // The main stack is unhosted because its window was closed -> re-open the main window
                if (stack.IsMainStack && Application.Current?.GetMainWindow() is { IsClosed: true } closedWindow)
                {
                    ReopenMainWindow(closedWindow, stack);
                    return stack ?? GetMainStack();
                }

                window = CreateWindow(new WindowCreationContext(WindowKind.Window, stack, content: stack.ContainerPage.Value, title: stack.Title));
                window.Closed += (_, _) => stack.Reset();

                // Open in (new) window
                if (!WindowManager.OpenWindow(window))
                    return null;

                activeStacks.Add(window, stack);
            }
            else
            {
                // Stack is already open in a window -> bring that window to the foreground
                if (Navigation.Windows.BringTargetWindowToFront)
                    window.Activate();
            }
        }

        return stack ?? GetMainStack();
    }

    /// <summary>
    /// Re-opens the main window for the (unhosted) main navigation stack, preserving the stack's
    /// current state. The fresh shell is created through the configured window factory.
    /// </summary>
    private void ReopenMainWindow(Window closedWindow, INavigationStack stack)
    {
        // Detach the stack container from the closed window's shell before re-parenting it
        closedWindow.Content = null;

        var window = CreateWindow(new WindowCreationContext(WindowKind.Window, stack, content: stack.ContainerPage.Value));
        if (!WindowManager.OpenWindow(window))
            throw new NavigationException("Failed to re-open the main window.");

        activeStacks[window] = stack;
        AppUtility.ReplaceMainWindow(window);
    }

    #endregion

    #endregion
}
