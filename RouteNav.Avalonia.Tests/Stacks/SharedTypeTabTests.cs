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

/// <summary>Regression tests for tab matching with factory-registered pages sharing a page type.</summary>
[Collection("Sequential")]
public class SharedTypeTabTests
{
    [Fact]
    public async Task PushAsync_FactoryTabsSharingPageType_SelectsMatchingTab()
    {
        var savedBaseRouteUri = Navigation.BaseRouteUri;
        IUIPlatform? savedPlatform = null;
        try
        {
            savedPlatform = Navigation.UIPlatform;
        }
        catch (Exception)
        {
            // not bootstrapped
        }

        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        Navigation.UIPlatform = new AvaloniaUIPlatform(new Lazy<IServiceProvider>(() => provider), services);
        Navigation.BaseRouteUri = new Uri("https://test.local");

        try
        {
            var stack = new TabbedPageStack("tabbed", "Tabbed");
            stack.AddPage("tab1", _ => new SharedTab());
            stack.AddPage("tab2", _ => new SharedTab());

            var page = await stack.PushAsync(new Uri("https://test.local/tabbed/tab2"));

            Assert.IsType<SharedTab>(page);
            var container = Assert.IsType<TabbedPageContainer>(stack.ContainerPage.Value);
            var selectedTab = Assert.IsType<TabItem>(container.TabControl!.SelectedItem);
            var selectedPage = Assert.IsType<SharedTab>(selectedTab.Content);

            // The selected tab must be the 'tab2' registration, not the first tab with a matching page type
            Assert.True(stack.EqualsRoutePath(selectedPage.RouteUri!, new Uri("https://test.local/tabbed/tab2")));
        }
        finally
        {
            Navigation.BaseRouteUri = savedBaseRouteUri;
            Navigation.UIPlatform = savedPlatform!;
        }
    }

    private sealed class SharedTab : Page
    {
    }
}
