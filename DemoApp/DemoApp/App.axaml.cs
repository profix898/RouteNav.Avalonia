using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using DemoApp.Pages;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Stacks;

namespace DemoApp;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Register pages
        Navigation.UIPlatform.RegisterPage<PageMain, Page1, Page2>();
        Navigation.UIPlatform.RegisterPage<Page3>();

        /* Main stack */
        var mainStack = new NavigationPageStack(Navigation.MainStackName);
        Navigation.UIPlatform.AddStack(mainStack);

        mainStack.AddPage<PageMain>(String.Empty); // MainPage (initial page)
        //mainStack.AddPage(String.Empty, () => new PageMain());

        mainStack.AddPage<Page1>("page1");
        mainStack.AddPage<Page2>("page2");

        mainStack.AddPage<NotFoundPage>("error404");
        mainStack.AddPage<InternalErrorPage>("error500");

        /* Second stack */
        var secondStack = new NavigationPageStack("second");
        Navigation.UIPlatform.AddStack(secondStack);

        secondStack.AddPage<Page3>(String.Empty);
        secondStack.AddPage<Page2>("page2");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ApplicationLifetime.SetMainWindow(new MainWindow());

        base.OnFrameworkInitializationCompleted();
    }
}