using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using DemoApp.Pages.Main;
using DemoApp.Pages.SidebarMenu;
using DemoApp.Pages.Tabbed;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Stacks;

namespace DemoApp;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Register pages with DI container
        // (optional, can be registered by other means e.g. directly with the DI container)
        Navigation.UIPlatform.RegisterPage<MainRootPage, MainPage1, MainPage2, MainPage3>();
        Navigation.UIPlatform.RegisterPage<SidebarMenuRootPage, SidebarMenuPage1>();
        Navigation.UIPlatform.RegisterPage<TabbedRootPage, TabbedPage1>();

        /* Main stack */
        // Create a new navigation stack (of type NavigationPageStack)
        var mainStack = new NavigationPageStack(Navigation.MainStackName, "DemoApp - Main");
        Navigation.UIPlatform.AddStack(mainStack);

        // Add main/root page (= initial page) to the stack (either by type or as factory method)
        mainStack.AddPage<MainRootPage>(String.Empty);
        //mainStack.AddPage(String.Empty, () => new MainRootPage());

        // Add more pages to the navigation stack (argument is the relative path to page on stack)
        mainStack.AddPage<MainPage1>("page1");
        mainStack.AddPage<MainPage2>("page2");
        mainStack.AddPage<MainPage3>("page3");

        // Add error pages (for testing purposes only)
        mainStack.AddPage<NotFoundPage>("error404");
        mainStack.AddPage<InternalErrorPage>("error500");

        /* SidebarMenu stack */
        var sidebarMenuStack = new SidebarMenuPageStack("sidebar", "DemoApp - Sidebar");
        Navigation.UIPlatform.AddStack(sidebarMenuStack);
        // SidebarMenu manages a 'SidebarMenuItem' collection, use .AddMenuItem() to add items
        sidebarMenuStack.AddMenuItem<SidebarMenuRootPage>(String.Empty, "RootPage");
        sidebarMenuStack.AddMenuItem<SidebarMenuPage1>("page1", "Page1");
        sidebarMenuStack.AddMenuItem("/tabbed/page1", "Tab Page1"); // Links to page on another stack

        /* Tabbed stack */
        var tabbedStack = new TabbedPageStack("tabbed", "DemoApp - Tabbed");
        Navigation.UIPlatform.AddStack(tabbedStack);
        tabbedStack.AddPage<TabbedRootPage>(String.Empty);
        tabbedStack.AddPage<TabbedPage1>("page1");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        // Set main window (window abstraction for desktop + mobile)
        ApplicationLifetime.SetMainWindow(new MainWindow());
    }
}