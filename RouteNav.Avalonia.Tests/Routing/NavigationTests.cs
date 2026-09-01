using System;
using RouteNav.Avalonia;
using Xunit;

namespace RouteNav.Avalonia.Tests.Routing;

/// <summary>Characterization tests for <see cref="Navigation.BuildRoute" /> (pin current behavior).</summary>
public class NavigationTests
{
    [Fact]
    public void BuildRoute_StackNameAndRelativeRoute()
    {
        Assert.Equal("https://avalonia.local/main/page?x=1", Navigation.BuildRoute("main", "page?x=1").ToString());
    }

    [Fact]
    public void BuildRoute_EmptyStackName()
    {
        Assert.Equal("https://avalonia.local/page", Navigation.BuildRoute(String.Empty, "page").ToString());
    }

    [Fact]
    public void BuildRoute_RelativeRouteUri()
    {
        Assert.Equal("https://avalonia.local/main/page", Navigation.BuildRoute("main", new Uri("page", UriKind.Relative)).ToString());
    }
}
