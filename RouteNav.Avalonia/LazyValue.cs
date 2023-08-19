using System;
using System.Diagnostics;

namespace RouteNav.Avalonia;

public sealed class LazyValue<T>
    where T : class
{
    private readonly Func<T> valueFactory;

    private T? value;

    public LazyValue(Func<T> valueFactory)
    {
        this.valueFactory = valueFactory;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public T Value
    {
        get { return value ??= valueFactory(); }
    }

    public void Reset()
    {
        value = null;
    }
}
