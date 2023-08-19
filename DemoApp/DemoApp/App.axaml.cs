using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using DemoApp.Pages;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Stacks;

namespace DemoApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Navigation.UIPlatform.RegisterPage<Page1, Page2>();

        var mainStack = new NavigationPageStack(Navigation.MainStackName);
        Navigation.UIPlatform.AddStack(mainStack);

        mainStack.AddPage<PageMain>(String.Empty); // MainPage (initial page)
        //mainStack.AddPage(String.Empty, () => new PageMain());

        mainStack.AddPage<Page1>("page1");
        mainStack.AddPage<Page2>("page2");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ApplicationLifetime.SetMainWindow(new MainWindow());

        base.OnFrameworkInitializationCompleted();
    }
}