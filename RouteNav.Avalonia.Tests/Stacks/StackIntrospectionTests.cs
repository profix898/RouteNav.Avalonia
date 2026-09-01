using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Platform;
using RouteNav.Avalonia.Stacks;
using Xunit;

namespace RouteNav.Avalonia.Tests.Stacks;

/// <summary>Tests for the stack introspection surface (registered/active stacks).</summary>
[Collection("Sequential")]
public class StackIntrospectionTests : IDisposable
{
    private readonly AvaloniaUIPlatform platform;
    private readonly IServiceProvider provider;

    public StackIntrospectionTests()
    {
        var services = new ServiceCollection();
        provider = services.BuildServiceProvider();
        platform = new AvaloniaUIPlatform(new Lazy<IServiceProvider>(() => provider), services);
        Navigation.UIPlatform = platform;
    }

    public void Dispose()
    {
        Navigation.UIPlatform = null!;
    }

    [Fact]
    public void RegisteredStacks_ReflectsRegistrationOrder()
    {
        var main = new ContentPageStack("main", "Main");
        var sidebar = new ContentPageStack("sidebar", "Sidebar");
        var events = new RouteEventStack("events");

        platform.AddStack(main);
        platform.AddStack(sidebar);
        platform.AddStack(events);

        var stacks = platform.RegisteredStacks;

        Assert.Equal(3, stacks.Count);
        Assert.Equal(["main", "sidebar", "events"], stacks.Select(s => s.Name).ToArray());
        Assert.IsType<RouteEventStack>(stacks[2]);
    }

    [Fact]
    public void RegisteredStacks_IsSnapshot()
    {
        platform.AddStack(new ContentPageStack("main", "Main"));

        var stacks = platform.RegisteredStacks;
        Assert.Single(stacks);

        platform.AddStack(new ContentPageStack("other", "Other"));

        Assert.Single(stacks);
        Assert.Equal(2, platform.RegisteredStacks.Count);
    }

    [Fact]
    public void ActiveStacks_IsEmptyWithoutHosting()
    {
        platform.AddStack(new ContentPageStack("main", "Main"));

        Assert.Empty(platform.ActiveStacks);
    }

    [Fact]
    public void RegisteredStacks_ExposesRegisteredRoutes()
    {
        var main = new ContentPageStack("main", "Main");
        main.AddPage("about", typeof(StubPage));
        platform.AddStack(main);

        var routes = platform.RegisteredStacks.Single().RegisteredRoutes;

        var route = Assert.Single(routes);
        Assert.Equal("about", route.RoutePath);
        Assert.Equal(typeof(StubPage), route.PageType);
    }

    private sealed class StubPage : Page
    {
    }
}
