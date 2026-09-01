using System;
using RouteNav.Avalonia.Stacks;
using Xunit;

namespace RouteNav.Avalonia.Tests.Routing;

/// <summary>Characterization tests for route building/parsing extensions (pin current behavior).</summary>
public class NavigationStackExtensionsTests
{
    private readonly ContentPageStack stack = new("sidebar", "Sidebar");

    #region BuildRoute

    [Fact]
    public void BuildRoute_RelativeString()
    {
        Assert.Equal("https://avalonia.local/sidebar/page", stack.BuildRoute("page").ToString());
    }

    [Fact]
    public void BuildRoute_RelativeStringWithQuery()
    {
        Assert.Equal("https://avalonia.local/sidebar/page?x=1", stack.BuildRoute("page?x=1").ToString());
    }

    [Fact]
    public void BuildRoute_EmptyRelativeRoute_ReturnsStackRootWithTrailingSlash()
    {
        Assert.Equal("https://avalonia.local/sidebar/", stack.BuildRoute(String.Empty).ToString());
    }

    [Fact]
    public void BuildRoute_AbsoluteRelativeUri_ReturnsAsIs()
    {
        var absolute = new Uri("https://example.com/other");

        Assert.Equal(absolute, stack.BuildRoute(absolute));
    }

    [Fact]
    public void BuildRoute_RelativeUri()
    {
        var relative = new Uri("page", UriKind.Relative);

        Assert.Equal("https://avalonia.local/sidebar/page", stack.BuildRoute(relative).ToString());
    }

    #endregion

    #region GetRoutePath

    [Fact]
    public void GetRoutePath_AbsoluteRouteOnStack()
    {
        var path = stack.GetRoutePath(stack.BuildRoute("page?x=1"), out var query);

        Assert.Equal("page", path);
        Assert.Equal("?x=1", query);
    }

    [Fact]
    public void GetRoutePath_AbsoluteRouteOnStack_NoQuery()
    {
        var path = stack.GetRoutePath(stack.BuildRoute("page"), out var query);

        Assert.Equal("page", path);
        Assert.Equal(String.Empty, query);
    }

    [Fact]
    public void GetRoutePath_AbsoluteForeignStack_ReturnsAbsolutePath()
    {
        var path = stack.GetRoutePath(new Uri("https://avalonia.local/other/page?x=1"), out _);

        Assert.Equal("/other/page", path);
    }

    [Fact]
    public void GetRoutePath_RelativeUri()
    {
        var path = stack.GetRoutePath(new Uri("page?x=1", UriKind.Relative), out var query);

        Assert.Equal("page", path);
        Assert.Equal("?x=1", query);
    }

    [Fact]
    public void GetRoutePath_EmptyRelativeUri_ReturnsEmpty()
    {
        var path = stack.GetRoutePath(new Uri(String.Empty, UriKind.Relative), out var query);

        Assert.Equal(String.Empty, path);
        Assert.Equal(String.Empty, query);
    }

    [Fact]
    public void GetRoutePath_QueryOnlyRelativeUri_ReturnsEmptyPath()
    {
        var path = stack.GetRoutePath(new Uri("?x=1", UriKind.Relative), out var query);

        Assert.Equal(String.Empty, path);
        Assert.Equal("?x=1", query);
    }

    [Fact]
    public void GetRoutePath_TrailingSlash_Trimmed()
    {
        Assert.Equal("page", stack.GetRoutePath(stack.BuildRoute("page/")));
    }

    [Fact]
    public void GetRoutePath_StackRoot_ReturnsEmpty()
    {
        Assert.Equal(String.Empty, stack.GetRoutePath(stack.BuildRoute(String.Empty)));
    }

    #endregion

    #region GetStackName

    [Fact]
    public void GetStackName_MultiSegment()
    {
        Assert.Equal("main", new Uri("https://avalonia.local/main/page").GetStackName());
    }

    [Fact]
    public void GetStackName_SingleSegment()
    {
        Assert.Equal("page", new Uri("https://avalonia.local/page").GetStackName());
    }

    [Fact]
    public void GetStackName_RelativeUri_ReturnsNull()
    {
        Assert.Null(new Uri("page", UriKind.Relative).GetStackName());
    }

    #endregion

    #region IsRouteOnStack

    [Fact]
    public void IsRouteOnStack_RelativeRoute_AssumesSameStack()
    {
        Assert.True(new Uri("page", UriKind.Relative).IsRouteOnStack(stack));
    }

    [Fact]
    public void IsRouteOnStack_MatchingStackName_True()
    {
        Assert.True(stack.BuildRoute("page").IsRouteOnStack(stack));
    }

    [Fact]
    public void IsRouteOnStack_OtherStackName_False()
    {
        Assert.False(new Uri("https://avalonia.local/other/page").IsRouteOnStack(stack));
    }

    #endregion

    #region EqualsRoutePath

    [Fact]
    public void EqualsRoutePath_SamePathDifferentQuery_True()
    {
        Assert.True(stack.EqualsRoutePath(stack.BuildRoute("page?x=1"), stack.BuildRoute("page?y=2")));
    }

    [Fact]
    public void EqualsRoutePath_DifferentPaths_False()
    {
        Assert.False(stack.EqualsRoutePath(stack.BuildRoute("pageA"), stack.BuildRoute("pageB")));
    }

    #endregion
}
