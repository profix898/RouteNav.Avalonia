using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;

namespace RouteNav.Avalonia.Routing;

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

    #region Nested Type: QueryHelpers

    /// <summary>Provides methods for manipulating query strings.</summary>
    /// <remarks>Cloned from 'Microsoft.AspNetCore.WebUtilities' (https://github.com/dotnet/aspnetcore/blob/main/src/Http/WebUtilities/src/QueryHelpers.cs).</remarks>
    private static class QueryHelpers
    {
        public static string AddQueryString(string uri, string name, string value)
        {
            ArgumentNullException.ThrowIfNull(uri);
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            return AddQueryString(uri, new[] { new KeyValuePair<string, string?>(name, value) });
        }

        public static string AddQueryString(string uri, IDictionary<string, string?> queryString)
        {
            ArgumentNullException.ThrowIfNull(uri);
            ArgumentNullException.ThrowIfNull(queryString);

            return AddQueryString(uri, (IEnumerable<KeyValuePair<string, string?>>) queryString);
        }

        public static string AddQueryString(string uri, IEnumerable<KeyValuePair<string, string?>> queryString)
        {
            ArgumentNullException.ThrowIfNull(uri);
            ArgumentNullException.ThrowIfNull(queryString);

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

            var sb = new StringBuilder();
            sb.Append(uriToBeAppended);
            foreach (var parameter in queryString)
            {
                if (parameter.Value == null)
                    continue;

                sb.Append(hasQuery ? '&' : '?');
                sb.Append(UrlEncoder.Default.Encode(parameter.Key));
                sb.Append('=');
                sb.Append(UrlEncoder.Default.Encode(parameter.Value));
                hasQuery = true;
            }

            sb.Append(anchorText);
            return sb.ToString();
        }
    }

    #endregion
}
