namespace RouteNav.Avalonia;

public sealed class NavigationEventArgs<T>
{
    public NavigationEventArgs(T? from, T? to)
    {
        From = from;
        To = to;
    }

    public T? From { get; }

    public T? To { get; }
}
