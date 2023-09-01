using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Platform;

public class AvaloniaUIPlatform : IUIPlatform
{
    private readonly Dictionary<string, INavigationStack> navigationStacks = new Dictionary<string, INavigationStack>();
    private readonly Dictionary<Window, INavigationStack> activeStacks = new Dictionary<Window, INavigationStack>();

    private readonly IServiceCollection serviceCollection;
    private readonly IServiceProvider serviceProvider;

    public AvaloniaUIPlatform(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
        : this(new AvaloniaWindowManager(), serviceCollection, serviceProvider)
    {
    }

    public AvaloniaUIPlatform(IWindowManager windowManager, IServiceCollection serviceCollection, IServiceProvider serviceProvider)
    {
        WindowManager = windowManager;

        this.serviceCollection = serviceCollection ?? throw new NotSupportedException("Navigation DI container (IServiceCollection) is not available.");
        this.serviceProvider = serviceProvider ?? throw new NotSupportedException("Navigation DI container (IServiceProvider) is not available.");

        RegisterPage(typeof(NotFoundPage), typeof(InternalErrorPage));
    }

    #region IUIPlatform Members

    public IWindowManager WindowManager { get; }

    #region Pages

    public void RegisterPage(params Type[] pageTypes)
    {
        if (pageTypes == null)
            throw new ArgumentNullException(nameof(pageTypes));

        foreach (var pageType in pageTypes)
        {
            if (!pageType.IsSubclassOf(typeof(Page)))
                throw new ArgumentException($"Type '{pageType.FullName}' is not a page.", nameof(pageTypes));

            serviceCollection.AddTransient(pageType, pageType);
        }
    }

    public Page GetPage(Type pageType, Uri uri, params object[] parameters)
    {
        if (pageType == null)
            throw new ArgumentNullException(nameof(pageType));

        //var page = (Page) serviceProvider.GetRequiredService(pageType);
        var page = (Page) ActivatorUtilities.CreateInstance(serviceProvider, pageType, parameters);

        // Supply page with query parameters (if available)
        page.PageQuery = uri.ParseQueryString();
        page.PageQuery.Add("routeUri", uri.ToString());

        return page;
    }

    #endregion

    #region Stacks

    public void AddStack(INavigationStack stack)
    {
        if (stack == null)
            throw new ArgumentNullException(nameof(stack));

        navigationStacks.Add(stack.Name, stack);
    }

    public void RemoveStack(string stackName)
    {
        if (activeStacks.Any(stack => stack.Value.Name.Equals(stackName)))
            throw new ArgumentException($"Stack '{stackName}' is currently in use.", nameof(stackName));

        navigationStacks.Remove(stackName);
    }

    public INavigationStack? GetStack(string stackName)
    {
        return navigationStacks.ContainsKey(stackName) ? navigationStacks[stackName] : null;
    }

    #endregion

    #region ActiveStacks

    public INavigationStack GetMainStack()
    {
        if (navigationStacks.TryGetValue(Navigation.MainStackName, out var mainStack))
            return mainStack;

        return activeStacks.Values.FirstOrDefault()
               ?? throw new NavigationException($"Stack '{Navigation.MainStackName}' is not available.");
    }

    public INavigationStack? GetActiveStack(string stackName)
    {
        if (String.IsNullOrEmpty(stackName))
            return GetMainStack();

        return activeStacks.FirstOrDefault(stack => stack.Value.Name.Equals(stackName)).Value;
    }

    public INavigationStack? GetActiveStackFromWindow(Window? window)
    {
        window ??= Application.Current!.GetMainWindow();

        return activeStacks.TryGetValue(window, out var stack) ? stack : null;
    }

    public Window? GetActiveWindowFromStack(INavigationStack? navigationStack)
    {
        navigationStack ??= GetMainStack();

        if (activeStacks.ContainsValue(navigationStack))
            return activeStacks.First(stackWindow => stackWindow.Value == navigationStack).Key;

        return null;
    }

    #endregion

    #region Targets

    public INavigationStack? ActivateStack(string stackName, INavigationStack? sourceStack = null)
    {
        if (String.IsNullOrEmpty(stackName))
            return null;

        var stack = GetStack(stackName);
        if (stack != null && !stack.IsEventStack)
        {
            var window = GetActiveWindowFromStack(stack);
            var sourceWindow = (sourceStack != null) ? GetActiveWindowFromStack(sourceStack) : null;
            if (window == null)
            {
                // Display in window associated with sourceStack or fall back to main/first application window
                sourceWindow ??= Application.Current?.GetMainWindow();
                if (sourceWindow == null)
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
            }
        }

        return stack ?? GetMainStack();
    }

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
                window = Window.Create(stack.ContainerPage.Value, stack.Title);
                window.Closed += (_, _) =>
                {
                    activeStacks.Remove(window);
                    stack.Reset();
                };

                // Open in (new) window
                if (!WindowManager.OpenWindow(window))
                    return null;

                activeStacks.Add(window, stack);
            }
        }

        return stack ?? GetMainStack();
    }

    #endregion

    #endregion
}
