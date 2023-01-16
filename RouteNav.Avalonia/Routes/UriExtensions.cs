using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;

namespace NSE.RouteNav.Routes;

public static class UriExtensions
{
    public static Uri AddQueryString(this Uri uri, string name, string value)
    {
        return new Uri(QueryHelpers.AddQueryString(uri.ToString(), name, value));
    }

    public static Uri AddQueryString(this Uri uri, IDictionary<string, string> queryParameters)
    {
        return new Uri(QueryHelpers.AddQueryString(uri.ToString(), queryParameters));
    }
}