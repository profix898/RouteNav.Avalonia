using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DialogHostAvalonia;
using Microsoft.Extensions.DependencyInjection;
using NSE.RouteNav.Bootstrap;
using NSE.RouteNav.Stacks;

namespace NSE.RouteNav.Platform;

public class AvaloniaUIPlatform : IUIPlatform
{
    private readonly Dictionary<string, INavigationStack> navigationStacks = new Dictionary<string, INavigationStack>();
    private readonly Dictionary<Window, INavigationStack> activeStacks = new Dictionary<Window, INavigationStack>();

    #region IUIPlatform Members

    #region Pages

    public void RegisterPages(params Type[] pageTypes)
    {
        if (pageTypes == null)
            throw new ArgumentNullException(nameof(pageTypes));

        var serviceCollection = AvaloniaLocator.Current.GetService<IServiceCollection>();
        if (serviceCollection == null)
            throw new NotSupportedException("Failed to register page because navigation DI container (IServiceCollection) is not available via AvaloniaLocator.");

        foreach (var pageType in pageTypes)
        {
            if (!pageType.IsSubclassOf(typeof(Page)))
                throw new ArgumentException($"Type '{pageType.FullName}' is not a page.", nameof(pageTypes));

            serviceCollection.AddTransient(pageType, pageType);
        }
    }

    public Page GetPage(Type pageType)
    {
        if (pageType == null)
            throw new ArgumentNullException(nameof(pageType));

        var serviceProvider = AvaloniaLocator.Current.GetService<IServiceProvider>();
        if (serviceProvider == null)
            throw new NotSupportedException("Failed to fetch page because navigation DI container (IServiceProvider) is not available via AvaloniaLocator.");

        try
        {
            return (Page)serviceProvider.GetRequiredService(pageType);
        }
        catch (Exception)
        {
            Debug.WriteLine($"Warning: Page of type '{pageType}' is not registered.");

            return (Page)ActivatorUtilities.CreateInstance(serviceProvider, pageType);
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

    public INavigationStack? PushToView(string stackName, INavigationStack? sourceStack = null)
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

    public INavigationStack? PushToDialog(string stackName, out Window? parentWindow, INavigationStack? sourceStack = null)
    {
        parentWindow = null;

        if (String.IsNullOrEmpty(stackName))
            return null;

        var stack = GetStack(stackName);
        if (stack != null && !stack.IsEventStack)
        {
            parentWindow = GetActiveWindowFromStack(stack);
            if (parentWindow == null)
            {
                // Display in window associated with sourceStack or fall back to main/first application window
                parentWindow = (sourceStack != null) ? GetActiveWindowFromStack(sourceStack) : Application.Current?.GetMainWindow();
                if (parentWindow == null)
                    throw new NavigationException("No main window/view available. Application not fully initialized yet.");
            }

            return stack;
        }

        return null;
    }

    public INavigationStack? PushToWindow(string stackName)
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
                if (!OpenWindow(window))
                    return null;
            }
        }

        return stack ?? GetMainStack();
    }

    #endregion

    #endregion

    private Task ShowDialog(Dialog dialog, Window? parentWindow = null)
    {
        var tcs = new TaskCompletionSource();

        // Check status, dialog overlay open?, ...
        if (parentWindow)
        {
            PushToNavCtrl();
        }
        else
        {
            var x = OpenDialogWindow();
            x.PushToNavCtrl();
        }

        var uiPlatform = AvaloniaLocator.Current.GetService<IUIPlatform>()
                         ?? throw new NavigationException($"Implementation of {nameof(IUIPlatform)} is not available. Bootstrap via {nameof(AppBuilderExtensions.UseRouteNavPlatform)}().");
        uiPlatform.OpenDialog(messageDialog, this); // TODO: Replaces any open dialog (dangerous!)

        return tcs.Task;
    }

    public virtual Task OpenDialog(Dialog dialog, Window? parentWindow = null)
    {
        parentWindow ??= Application.Current?.GetMainWindow();
        if (parentWindow == null)
            throw new NavigationException("No main window/view available. Application not fully initialized yet.");

        if (parentWindow.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime)
        {
            // Open in window
            var platformDialog = new Avalonia.Controls.Window
            {
                Title = dialog.Title,
                Width = dialog.Size.Width,
                Height = dialog.Size.Height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SystemDecorations = SystemDecorations.BorderOnly,

                // ContentControl
                Content = dialog,
                HorizontalContentAlignment = dialog.HorizontalContentAlignment,
                VerticalContentAlignment = dialog.VerticalContentAlignment,

                // TemplatedControl
                Background = dialog.Background,
                BorderBrush = dialog.BorderBrush,
                BorderThickness = dialog.BorderThickness,
                CornerRadius = dialog.CornerRadius,
                FontFamily = dialog.FontFamily,
                FontSize = dialog.FontSize,
                FontStyle = dialog.FontStyle,
                FontWeight = dialog.FontWeight,
                FontStretch = dialog.FontStretch,
                Foreground = dialog.Foreground,
                Padding = dialog.Padding,

                // Control
                Tag = dialog,
                ContextMenu = dialog.ContextMenu,
                ContextFlyout = dialog.ContextFlyout
            };
            platformDialog.Opened += (_, _) => dialog.OnOpened();
            platformDialog.Closed += (_, _) => dialog.OnClosed();

            return platformDialog.ShowDialog(parentWindow.PlatformWindow);
        }

        if (parentWindow.ApplicationLifetime is ISingleViewApplicationLifetime)
        {
            // Open in overlay (on parentWindow)
            return DialogHost.Show(dialog, parentWindow.DialogHost, (_, _) => dialog.OnOpened(), (_, _) => dialog.OnClosed());
        }

        throw new NotSupportedException($"IApplicationLifetime of type '{parentWindow.ApplicationLifetime.GetType()}' not supported.");
    }

    public virtual bool OpenWindow(Window window, Window? parentWindow = null)
    {
        var mainWindow = Application.Current?.GetMainWindow();
        if (mainWindow == null)
            throw new NavigationException("No main window/view available. Application not fully initialized yet.");

        parentWindow ??= mainWindow;

        if (mainWindow.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            // Open in window
            var platformWindow = window.ToPlatformWindow(desktopLifetime);
            platformWindow.Show(parentWindow.PlatformWindow ?? mainWindow.PlatformWindow);

            return true;
        }

        if (mainWindow.ApplicationLifetime is ISingleViewApplicationLifetime)
        {
            return false;
        }

        throw new NotSupportedException($"IApplicationLifetime of type '{mainWindow.ApplicationLifetime.GetType()}' not supported.");
    }
}
