using System;
using System.Diagnostics;

namespace RouteNav.Avalonia;

/// <summary>A lightweight, resettable lazy value holder.</summary>
/// <typeparam name="T">The reference type to lazily create.</typeparam>
public sealed class LazyValue<T>
    where T : class
{
    private readonly Func<T> valueFactory;

    private T? value;

    /// <summary>Initializes a new instance of the <see cref="LazyValue{T}" /> class.</summary>
    public LazyValue(Func<T> valueFactory)
    {
        this.valueFactory = valueFactory;
    }

    /// <summary>Gets the value, creating it on first access.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public T Value
    {
        get { return value ??= valueFactory(); }
    }

    /// <summary>Clears the cached value so it is re-created on next access.</summary>
    public void Reset()
    {
        value = null;
    }
}
