using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Platform;
using RouteNav.Avalonia.Tests.Stacks;
using Xunit;

namespace RouteNav.Avalonia.Tests.Platform;

/// <summary>Tests for page instantiation via <see cref="AvaloniaUIPlatform.GetPage" />.</summary>
[Collection("Sequential")]
public class AvaloniaUIPlatformTests
{
    [Fact]
    public void GetPage_QueryContainingRouteUri_DoesNotCrashAndPreservesBoth()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var platform = new AvaloniaUIPlatform(new Lazy<IServiceProvider>(() => provider), services);

        var uri = new Uri("https://test.local/main/details?routeUri=evil&y=1");
        var page = platform.GetPage(typeof(StubPage), uri);

        Assert.IsType<StubPage>(page); // Not an error page
        Assert.Equal("1", page.PageQuery["y"]);
        Assert.Equal(uri.ToString(), page.PageQuery["routeUri"]);
        Assert.Equal(uri, page.RouteUri);
    }

    private sealed class StubPage : Page
    {
    }
}
