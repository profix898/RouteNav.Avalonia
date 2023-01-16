using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using DemoApp.Pages;
using NSE.RouteNav;
using NSE.RouteNav.Bootstrap;
using NSE.RouteNav.Stacks;

namespace DemoApp
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            Navigation.UIPlatform.RegisterPages(typeof(Page1), typeof(Page2));

            var mainStack = new NavigationPageStack(Navigation.MainStackName);
            Navigation.UIPlatform.AddStack(mainStack);

            mainStack.AddPage<PageMain>(String.Empty); // MainPage (initial page)
            //mainStack.AddPage(String.Empty, () => new PageMain());

            mainStack.AddPage<Page1>("/page1");
            mainStack.AddPage<Page2>("/page2");
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ApplicationLifetime.SetMainWindow(new MainWindow());

            base.OnFrameworkInitializationCompleted();
        }
    }
}