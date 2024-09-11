using System;

namespace RouteNav.Avalonia.Internal;

internal sealed class Disposable : IDisposable
{
    private readonly IDisposable[] disposables;

    private Disposable(IDisposable[] disposables)
    {
        this.disposables = disposables;
    }

    #region Implementation of IDisposable

    public void Dispose()
    {
        foreach (var disposable in disposables)
            disposable.Dispose();
    }

    #endregion

    public static IDisposable Create(params IDisposable[] disposables)
    {
        return new Disposable(disposables);
    }
}
