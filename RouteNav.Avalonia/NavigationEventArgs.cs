namespace RouteNav.Avalonia;

/// <summary>Event data describing a navigation transition from one item to another.</summary>
/// <typeparam name="T">The navigated item type (e.g. <see cref="Page"/> or <see cref="Dialog"/>).</typeparam>
public sealed class NavigationEventArgs<T>
{
    /// <summary>Initializes a new instance of the <see cref="NavigationEventArgs{T}"/> class.</summary>
    public NavigationEventArgs(T? from, T? to)
    {
        From = from;
        To = to;
    }

    /// <summary>Gets the item navigated away from, if any.</summary>
    public T? From { get; }

    /// <summary>Gets the item navigated to, if any.</summary>
    public T? To { get; }
}
