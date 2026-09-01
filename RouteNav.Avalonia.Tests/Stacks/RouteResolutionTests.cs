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

/// <summary>Regression tests for route resolution edge cases found during the API review.</summary>
[Collection("Sequential")]
public class RouteResolutionTests
{
    [Fact]
    public async Task PushAsync_FragmentOnlyRoute_ResolvesStackRoot()
    {
        var stack = CreateStack(out var restore);
        try
        {
            stack.AddPage(String.Empty, _ => new ResolvedPage());

            var page = await stack.PushAsync(new Uri("https://test.local/tabbed/#anchor"));

            Assert.IsType<ResolvedPage>(page);
        }
        finally
        {
            restore();
        }
    }

    [Fact]
    public async Task PushAsync_FragmentAfterPath_ResolvesPath()
    {
        var stack = CreateStack(out var restore);
        try
        {
            stack.AddPage("details", _ => new ResolvedPage());

            var page = await stack.PushAsync(new Uri("https://test.local/tabbed/details#section"));

            Assert.IsType<ResolvedPage>(page);
        }
        finally
        {
            restore();
        }
    }

    [Fact]
    public async Task AddPage_WithQuery_ResolvesByPath()
    {
        var stack = CreateStack(out var restore);
        try
        {
            stack.AddPage("details?x=1", _ => new ResolvedPage());

            var page = await stack.PushAsync(stack.BuildRoute("details?x=1"));

            Assert.IsType<ResolvedPage>(page);
        }
        finally
        {
            restore();
        }
    }

    [Fact]
    public async Task AddPage_TrailingSlashBeforeQuery_ResolvesByPath()
    {
        var stack = CreateStack(out var restore);
        try
        {
            stack.AddPage("details/?x=1", _ => new ResolvedPage());

            var page = await stack.PushAsync(stack.BuildRoute("details/?x=1"));
            Assert.IsType<ResolvedPage>(page);

            var page2 = await stack.PushAsync(stack.BuildRoute("details"));
            Assert.IsType<ResolvedPage>(page2);
        }
        finally
        {
            restore();
        }
    }

    [Fact]
    public void GetStackName_HostRoot_ReturnsEmpty()
    {
        Assert.Equal(String.Empty, new Uri("https://avalonia.local/").GetStackName());
    }

    [Fact]
    public void IsRouteOnStack_HostRoot_TargetsMainStackOnly()
    {
        var main = new ContentPageStack("main", "Main");
        var sidebar = new ContentPageStack("sidebar", "Sidebar");
        var rootUri = new Uri("https://avalonia.local/");

        Assert.True(rootUri.IsRouteOnStack(main));
        Assert.False(rootUri.IsRouteOnStack(sidebar));
    }

    private static ContentPageStack CreateStack(out Action restore)
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

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var provider = services.BuildServiceProvider();
        Navigation.UIPlatform = new AvaloniaUIPlatform(new Lazy<IServiceProvider>(() => provider), services);
        Navigation.BaseRouteUri = new Uri("https://test.local");

        restore = () =>
        {
            Navigation.BaseRouteUri = savedBaseRouteUri;
            Navigation.UIPlatform = savedPlatform!;
        };

        return new ContentPageStack("tabbed", "Tabbed");
    }

    private sealed class ResolvedPage : Page
    {
    }
}
