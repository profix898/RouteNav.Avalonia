using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Platform;
using RouteNav.Avalonia.Stacks;
using RouteNav.Avalonia.StackContainers;
using Xunit;

namespace RouteNav.Avalonia.Tests.Stacks;

/// <summary>Headless tests for route-based pushes on a TabbedPageStack (no window hosting required).</summary>
[Collection("Sequential")]
public class TabbedPageStackTests
{
    [Fact]
    public async Task PushAsync_RouteUri_ResolvesPageAndSelectsMatchingTab()
    {
        var (stack, restore) = CreateStack();
        try
        {
            stack.AddPage<TabRootPage>(String.Empty);
            stack.AddPage<TabPageTwo>("page2");

            var page = await stack.PushAsync(new Uri("https://test.local/tabbed/page2"));

            Assert.IsType<TabPageTwo>(page);
            Assert.IsType<TabPageTwo>(stack.CurrentPage);

            var container = Assert.IsType<TabbedPageContainer>(stack.ContainerPage.Value);
            var selectedTab = Assert.IsType<TabItem>(container.TabControl!.SelectedItem);
            Assert.IsType<TabPageTwo>(selectedTab.Content);
        }
        finally
        {
            restore();
        }
    }

    [Fact]
    public async Task PushAsync_RelativeRouteUri_SelectsMatchingTab()
    {
        var (stack, restore) = CreateStack();
        try
        {
            stack.AddPage<TabRootPage>(String.Empty);
            stack.AddPage<TabPageTwo>("page2");

            await stack.PushAsync(new Uri("page2", UriKind.Relative));

            var container = (TabbedPageContainer)stack.ContainerPage.Value;
            var selectedTab = Assert.IsType<TabItem>(container.TabControl!.SelectedItem);
            var selectedPage = Assert.IsType<TabPageTwo>(selectedTab.Content);
            Assert.True(stack.EqualsRoutePath(selectedPage.RouteUri!, stack.CurrentPage!.RouteUri!));
        }
        finally
        {
            restore();
        }
    }

    private static (TabbedPageStack, Action) CreateStack()
    {
        var savedBaseRouteUri = Navigation.BaseRouteUri;
        var savedUiPlatform = TryGetUiPlatform();

        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        Navigation.UIPlatform = new AvaloniaUIPlatform(new Lazy<IServiceProvider>(() => provider), services);
        Navigation.BaseRouteUri = new Uri("https://test.local");

        return (new TabbedPageStack("tabbed", "Tabbed"), Restore);

        void Restore()
        {
            Navigation.BaseRouteUri = savedBaseRouteUri;
            Navigation.UIPlatform = savedUiPlatform!;
        }
    }

    private static IUIPlatform? TryGetUiPlatform()
    {
        try
        {
            return Navigation.UIPlatform;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private sealed class TabRootPage : Page
    {
    }

    private sealed class TabPageTwo : Page
    {
    }
}
