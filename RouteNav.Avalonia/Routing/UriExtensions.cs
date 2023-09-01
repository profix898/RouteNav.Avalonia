using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;

namespace RouteNav.Avalonia.Routing;

public static class UriExtensions
{
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
    /// <remarks>From: StackOverflow - Get URL parameters from a string in .NET (https://stackoverflow.com/a/20134983).</remarks>
    public static Dictionary<string, string> ParseQueryString(this Uri uri)
    {
        if (!uri.IsAbsoluteUri)
            uri = new Uri(Navigation.BaseRouteUri, uri); // ParseQueryString support only absolute URIs

        if (uri.Query.Length == 0)
            return new Dictionary<string, string>();

        return uri.Query.TrimStart('?')
                  .Split(new[] { '&', ';' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(parameter => parameter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
                  .GroupBy(parts => parts[0],
                           parts => parts.Length > 2 ? String.Join("=", parts.Select(Uri.UnescapeDataString), 1, parts.Length - 1) : parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "")
                  .ToDictionary(grouping => grouping.Key,
                                grouping => String.Join(",", grouping));
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
