using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia.Pages;
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

    public Page GetPage(Type pageType, params object[] parameters)
    {
        if (pageType == null)
            throw new ArgumentNullException(nameof(pageType));

        try
        {
            return (Page) serviceProvider.GetRequiredService(pageType);
        }
        catch (Exception)
        {
            Debug.WriteLine($"Warning: Page of type '{pageType}' is not registered.");

            return (Page) ActivatorUtilities.CreateInstance(serviceProvider, pageType, parameters);
        }
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
        var mainStack = activeStacks.Values.FirstOrDefault();
        if (mainStack == null)
            navigationStacks.TryGetValue(Navigation.MainStackName, out mainStack);

        return mainStack ?? throw new NavigationException($"Stack '{Navigation.MainStackName}' is not available.");
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
            if (window == null)
            {
                // Display in window associated with sourceStack or fall back to main/first application window
                var sourceWindow = (sourceStack != null) ? GetActiveWindowFromStack(sourceStack) : Application.Current?.GetMainWindow();
                if (sourceWindow == null)
                    throw new NavigationException("No main window/view available. Application not fully initialized yet.");

                // Associate initial/main window with stack
                if (activeStacks.Count == 0)
                    activeStacks.Add(sourceWindow, stack);

                // Switch current/active stack
                activeStacks[sourceWindow] = stack;
                sourceStack?.Reset();
                sourceWindow.SetContent(stack.ContainerPage.Value);
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
                window = new Window(stack.ContainerPage.Value);
                window.Closed += (_, _) => { stack.Reset(); };
                activeStacks.Add(window, stack);

                // Open in (new) window
                if (!WindowManager.OpenWindow(window))
                    return null;
            }
        }

        return stack ?? GetMainStack();
    }

    #endregion

    #endregion
}
