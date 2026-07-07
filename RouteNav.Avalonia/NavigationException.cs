using System;

namespace RouteNav.Avalonia;

/// <summary>The exception thrown when a navigation operation fails.</summary>
public class NavigationException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="NavigationException"/> class.</summary>
    public NavigationException(string? message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="NavigationException"/> class with an inner exception.</summary>
    public NavigationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}