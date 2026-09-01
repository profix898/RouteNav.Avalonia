using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;

namespace RouteNav.Avalonia.Routing;

/// <summary>URI helper methods for building and parsing RouteNav route URIs.</summary>
public static class UriExtensions
{
    /// <summary>Parses a route path into a route URI. Both relative paths (e.g. 'myPage' relative to current stack) and
    ///          absolute paths (e.g. '/myStack/myPage') are supported. The leading '/' denotes an absolute path.</summary>
    public static Uri ParseRoutePath(this string routePath)
    {
        return routePath.StartsWith('/')
            ? new Uri(Navigation.BaseRouteUri, routePath.TrimEnd('/'))
            : new Uri(routePath.TrimEnd('/'), UriKind.Relative);
    }

    /// <summary>Constructs a URI with query string (from a key/value pair).</summary>
    public static Uri AddQueryString(this Uri uri, string name, string value)
    {
        return new Uri(QueryHelpers.AddQueryString(uri.ToString(), name, value));
    }

    /// <summary>Constructs a URI with query string (from a key/value dictionary).</summary>
    public static Uri AddQueryString(this Uri uri, IDictionary<string, string> queryParameters)
    {
        return new Uri(QueryHelpers.AddQueryString(uri.ToString(), queryParameters));
    }

    /// <summary>Parses a URI query string into a key/value dictionary.</summary>
    /// <remarks>
    /// Duplicate keys are joined into a single comma-separated value; values containing '=' keep the embedded value.
    /// Keys are unescaped and matched ordinal (a raw key may map onto an existing key after unescaping); a parameter
    /// without a name yields an empty-string key.
    /// </remarks>
    public static Dictionary<string, string> ParseQueryString(this Uri uri)
    {
        if (!uri.IsAbsoluteUri)
            uri = new Uri(Navigation.BaseRouteUri, uri); // ParseQueryString support only absolute URIs

        var query = uri.Query;
        if (query.Length <= 1)
            return new Dictionary<string, string>();

        var parameters = query.AsSpan(1); // Skip leading '?'
        var result = new Dictionary<string, string>(CountParameters(parameters), StringComparer.Ordinal);

        while (!parameters.IsEmpty)
        {
            var separatorIndex = parameters.IndexOfAny('&', ';');
            ReadOnlySpan<char> pair;
            if (separatorIndex >= 0)
            {
                pair = parameters[..separatorIndex];
                parameters = parameters[(separatorIndex + 1)..];
            }
            else
            {
                pair = parameters;
                parameters = ReadOnlySpan<char>.Empty;
            }

            if (pair.IsEmpty)
                continue;

            var equalsIndex = pair.IndexOf('=');
            var keySpan = equalsIndex < 0 ? pair : pair[..equalsIndex];
            var valueSpan = equalsIndex < 0 ? ReadOnlySpan<char>.Empty : pair[(equalsIndex + 1)..];

            var key = Uri.UnescapeDataString(keySpan.ToString());
            var value = Uri.UnescapeDataString(valueSpan.ToString());
            if (result.TryGetValue(key, out var existing))
                result[key] = String.Concat(existing, ",", value);
            else
                result[key] = value;
        }

        return result;
    }

    /// <summary>Counts the number of query parameters in the given (query-only) span.</summary>
    private static int CountParameters(ReadOnlySpan<char> parameters)
    {
        var count = 1;
        foreach (var c in parameters)
        {
            if (c == '&' || c == ';')
                count++;
        }

        return count;
    }

    #region Nested Type: QueryHelpers

    /// <summary>Provides methods for manipulating query strings.</summary>
    /// <remarks>Cloned from 'Microsoft.AspNetCore.WebUtilities' (https://github.com/dotnet/aspnetcore/blob/main/src/Http/WebUtilities/src/QueryHelpers.cs).</remarks>
    private static class QueryHelpers
    {
        public static string AddQueryString(string uri, string name, string value)
        {
            return AddQueryString(uri, new[] { new KeyValuePair<string, string?>(name, value) });
        }

        public static string AddQueryString(string uri, IDictionary<string, string?> queryString)
        {
            return AddQueryString(uri, (IEnumerable<KeyValuePair<string, string?>>) queryString);
        }

        public static string AddQueryString(string uri, IEnumerable<KeyValuePair<string, string?>> queryString)
        {
            var anchorIndex = uri.IndexOf('#');
            var uriToBeAppended = uri.AsSpan();
            var anchorText = ReadOnlySpan<char>.Empty;

            // If there is an anchor, then the query string must be inserted before its first occurrence.
            if (anchorIndex != -1)
            {
                anchorText = uriToBeAppended.Slice(anchorIndex);
                uriToBeAppended = uriToBeAppended.Slice(0, anchorIndex);
            }

            var queryIndex = uriToBeAppended.IndexOf('?');
            var hasQuery = queryIndex != -1;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(uriToBeAppended);
            foreach (var parameter in queryString)
            {
                if (parameter.Value == null)
                    continue;

                stringBuilder.Append(hasQuery ? '&' : '?');
                stringBuilder.Append(UrlEncoder.Default.Encode(parameter.Key));
                stringBuilder.Append('=');
                stringBuilder.Append(UrlEncoder.Default.Encode(parameter.Value));
                hasQuery = true;
            }
            stringBuilder.Append(anchorText);

            return stringBuilder.ToString();
        }
    }

    #endregion
}
