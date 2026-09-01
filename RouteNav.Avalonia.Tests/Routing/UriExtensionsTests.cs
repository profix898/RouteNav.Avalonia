using System;
using System.Collections.Generic;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Routing;
using Xunit;

namespace RouteNav.Avalonia.Tests;

/// <summary>Characterization tests for <see cref="UriExtensions" /> (pin current behavior).</summary>
[Collection("Sequential")]
public class UriExtensionsTests
{
    #region ParseQueryString

    [Fact]
    public void ParseQueryString_NoQuery_ReturnsEmpty()
    {
        var uri = new Uri("https://avalonia.local/main/page");

        var query = uri.ParseQueryString();

        Assert.Empty(query);
    }

    [Fact]
    public void ParseQueryString_SingleParameter()
    {
        var uri = new Uri("https://avalonia.local/main/page?a=1");

        var query = uri.ParseQueryString();

        Assert.Single(query);
        Assert.Equal("1", query["a"]);
    }

    [Fact]
    public void ParseQueryString_MultipleParameters()
    {
        var uri = new Uri("https://avalonia.local/main/page?a=1&b=2&c=3");

        var query = uri.ParseQueryString();

        Assert.Equal(3, query.Count);
        Assert.Equal("1", query["a"]);
        Assert.Equal("2", query["b"]);
        Assert.Equal("3", query["c"]);
    }

    [Fact]
    public void ParseQueryString_DuplicateKeys_JoinedWithComma()
    {
        var uri = new Uri("https://avalonia.local/main/page?a=1&a=2");

        var query = uri.ParseQueryString();

        Assert.Single(query);
        Assert.Equal("1,2", query["a"]);
    }

    [Fact]
    public void ParseQueryString_MultipleEquals_JoinedWithEquals()
    {
        var uri = new Uri("https://avalonia.local/main/page?a=1=2");

        var query = uri.ParseQueryString();

        Assert.Equal("1=2", query["a"]);
    }

    [Fact]
    public void ParseQueryString_EmptyValue_ReturnsEmptyString()
    {
        var uri = new Uri("https://avalonia.local/main/page?a=");

        var query = uri.ParseQueryString();

        Assert.Single(query);
        Assert.Equal(String.Empty, query["a"]);
    }

    [Fact]
    public void ParseQueryString_SemicolonSeparator_Supported()
    {
        var uri = new Uri("https://avalonia.local/main/page?a=1;b=2");

        var query = uri.ParseQueryString();

        Assert.Equal(2, query.Count);
        Assert.Equal("1", query["a"]);
        Assert.Equal("2", query["b"]);
    }

    [Fact]
    public void ParseQueryString_UnescapesValues()
    {
        var uri = new Uri("https://avalonia.local/main/page?a=%20x%2By");

        var query = uri.ParseQueryString();

        Assert.Equal(" x+y", query["a"]);
    }

    [Fact]
    public void ParseQueryString_PlusIsNotDecodedAsSpace()
    {
        var uri = new Uri("https://avalonia.local/main/page?a=1+2");

        var query = uri.ParseQueryString();

        Assert.Equal("1+2", query["a"]);
    }

    [Fact]
    public void ParseQueryString_RelativeUri_ResolvedAgainstBaseRouteUri()
    {
        var uri = new Uri("page?a=1", UriKind.Relative);

        var query = uri.ParseQueryString();

        Assert.Single(query);
        Assert.Equal("1", query["a"]);
    }

    [Fact]
    public void ParseQueryString_UnescapesKeys_AndMergesDuplicatesAfterUnescaping()
    {
        var uri = new Uri("https://avalonia.local/main/page?k%20ey=a&k%20ey=b");

        var query = uri.ParseQueryString();

        Assert.Single(query);
        Assert.Equal("a,b", query["k ey"]);
    }

    [Fact]
    public void ParseQueryString_ParameterWithoutName_YieldsEmptyKey()
    {
        var uri = new Uri("https://avalonia.local/main/page?=v");

        var query = uri.ParseQueryString();

        Assert.Single(query);
        Assert.Equal("v", query[String.Empty]);
    }

    #endregion

    #region ParseRoutePath

    [Fact]
    public void ParseRoutePath_RelativePath_ReturnsRelativeUri()
    {
        var uri = "myPage".ParseRoutePath();

        Assert.False(uri.IsAbsoluteUri);
        Assert.Equal("myPage", uri.OriginalString);
    }

    [Fact]
    public void ParseRoutePath_AbsolutePath_ResolvedAgainstBaseRouteUri()
    {
        var uri = "/main/myPage".ParseRoutePath();

        Assert.True(uri.IsAbsoluteUri);
        Assert.Equal("https://avalonia.local/main/myPage", uri.ToString());
    }

    [Fact]
    public void ParseRoutePath_TrailingSlash_Trimmed()
    {
        Assert.Equal("myPage", "myPage/".ParseRoutePath().OriginalString);
    }

    #endregion

    #region AddQueryString

    [Fact]
    public void AddQueryString_SingleParameter()
    {
        var uri = new Uri("https://avalonia.local/main/page");

        var result = uri.AddQueryString("a", "1");

        Assert.Equal("https://avalonia.local/main/page?a=1", result.ToString());
    }

    [Fact]
    public void AddQueryString_AppendsToExistingQuery()
    {
        var uri = new Uri("https://avalonia.local/main/page?a=1");

        var result = uri.AddQueryString("b", "2");

        Assert.Equal("https://avalonia.local/main/page?a=1&b=2", result.ToString());
    }

    [Fact]
    public void AddQueryString_EncodesKeyAndValue()
    {
        var uri = new Uri("https://avalonia.local/main/page");

        var result = uri.AddQueryString("k ey", "v alue");

        // Uri.ToString() unescapes %20 -> ' '; compare the escaped form via AbsoluteUri
        Assert.Equal("https://avalonia.local/main/page?k%20ey=v%20alue", result.AbsoluteUri);
    }

    [Fact]
    public void AddQueryString_PreservesAnchor()
    {
        var uri = new Uri("https://avalonia.local/main/page#section");

        var result = uri.AddQueryString("a", "1");

        Assert.Equal("https://avalonia.local/main/page?a=1#section", result.ToString());
    }

    [Fact]
    public void AddQueryString_Dictionary_NullValuesAreSkipped()
    {
        var uri = new Uri("https://avalonia.local/main/page");
        var parameters = new Dictionary<string, string> { ["a"] = "1", ["b"] = null!, ["c"] = "3" };

        var result = uri.AddQueryString(parameters);

        Assert.Equal("https://avalonia.local/main/page?a=1&c=3", result.ToString());
    }

    #endregion
}
