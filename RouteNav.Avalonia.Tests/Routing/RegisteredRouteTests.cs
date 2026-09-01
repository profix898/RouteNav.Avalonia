using System;
using System.Linq;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Pages;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.Stacks;
using Xunit;

namespace RouteNav.Avalonia.Tests.Routing;

/// <summary>Tests for the read-only route registration view exposed by <see cref="INavigationStack.RegisteredRoutes" />.</summary>
public class RegisteredRouteTests
{
    [Fact]
    public void AddPage_Type_ExposesPageType()
    {
        var stack = new ContentPageStack("main", "Main");

        stack.AddPage<TestPage>("test");

        var route = Assert.Single(stack.RegisteredRoutes);
        Assert.Equal("test", route.RoutePath);
        Assert.Equal(typeof(TestPage), route.PageType);
        Assert.NotNull(route.PageFactory);
    }

    [Fact]
    public void AddPage_Factory_ExposesFactoryWithoutPageType()
    {
        var stack = new ContentPageStack("main", "Main");

        stack.AddPage("test", _ => new TestPage());

        var route = Assert.Single(stack.RegisteredRoutes);
        Assert.Equal("test", route.RoutePath);
        Assert.Null(route.PageType);
        Assert.NotNull(route.PageFactory);
    }

    [Fact]
    public void AddPage_TrimsTrailingSlashes()
    {
        var stack = new ContentPageStack("main", "Main");

        stack.AddPage("test/", typeof(TestPage));

        Assert.Equal("test", stack.RegisteredRoutes.Single().RoutePath);
    }

    [Fact]
    public void AddPage_ReRegistration_ReplacesRoute()
    {
        var stack = new ContentPageStack("main", "Main");

        stack.AddPage("test", typeof(TestPage));
        stack.AddPage("test", typeof(OtherPage));

        var route = Assert.Single(stack.RegisteredRoutes);
        Assert.Equal(typeof(OtherPage), route.PageType);
    }

    [Fact]
    public void RegisteredRoutes_IsLiveView()
    {
        var stack = new ContentPageStack("main", "Main");
        var routes = stack.RegisteredRoutes;
        Assert.Empty(routes);

        stack.AddPage("a", typeof(TestPage));
        stack.AddPage("b", typeof(TestPage));

        Assert.Equal(2, routes.Count);
        Assert.Equal(["a", "b"], routes.Select(r => r.RoutePath).ToArray());
    }

    [Fact]
    public void RouteEventStack_RegisteredRoutes_IsEmpty()
    {
        var stack = new RouteEventStack("events");

        Assert.Empty(stack.RegisteredRoutes);
    }

    private class TestPage : Page
    {
    }

    private class OtherPage : Page
    {
    }
}
