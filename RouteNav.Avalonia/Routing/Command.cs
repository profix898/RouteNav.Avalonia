using System;
using System.Windows.Input;

namespace RouteNav.Avalonia.Routing;

public class Command<T> : Command
{
    public Command(Action<T> execute)
        : base(o =>
        {
            if (IsValidParameter(o))
                execute((T) o);
        })
    {
    }

    public Command(Action<T> execute, Func<T, bool>? canExecute)
        : base(o =>
        {
            if (IsValidParameter(o))
                execute((T) o);
        }, o => IsValidParameter(o) && (canExecute?.Invoke((T) o) ?? true))
    {
    }

    private static bool IsValidParameter(object? parameter)
    {
        if (parameter != null)
            return parameter is T;

        var type = typeof(T);
        if (Nullable.GetUnderlyingType(type) != null)
            return true;

        return !type.IsValueType;
    }
}

public class Command : ICommand
{
    private readonly Func<object?, bool> canExecute;
    private readonly Action<object?> executeFunc;

    public Command(Action executeFunc, Func<bool>? canExecute = null)
        : this(_ => executeFunc(), _ => canExecute?.Invoke() ?? true)
    {
    }

    public Command(Action<object?> executeFunc, Func<object?, bool>? canExecute = null)
    {
        this.executeFunc = executeFunc;
        this.canExecute = canExecute ?? (_ => true);
    }

    #region Implementation of ICommand

    public bool CanExecute(object? parameter)
    {
        return canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        executeFunc(parameter);
    }

    public event EventHandler? CanExecuteChanged;

    #endregion

    public void ChangeCanExecute()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
