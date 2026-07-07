using System;
using System.Windows.Input;

namespace RouteNav.Avalonia.Routing;

/// <summary>A strongly-typed <see cref="ICommand"/> whose parameter is coerced to <typeparamref name="T"/>.</summary>
/// <typeparam name="T">The command parameter type.</typeparam>
public class Command<T> : Command
{
    /// <summary>Initializes a new instance of the <see cref="Command{T}"/> class.</summary>
    public Command(Action<T> execute)
        : base(o =>
        {
            if (IsValidParameter(o))
                execute((T) o);
        })
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Command{T}"/> class with a can-execute predicate.</summary>
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

/// <summary>A simple relay <see cref="ICommand"/> implementation.</summary>
public class Command : ICommand
{
    private readonly Func<object?, bool> canExecute;
    private readonly Action<object?> executeFunc;

    /// <summary>Initializes a new instance of the <see cref="Command"/> class from parameterless delegates.</summary>
    public Command(Action executeFunc, Func<bool>? canExecute = null)
        : this(_ => executeFunc(), _ => canExecute?.Invoke() ?? true)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Command"/> class from parameterized delegates.</summary>
    public Command(Action<object?> executeFunc, Func<object?, bool>? canExecute = null)
    {
        this.executeFunc = executeFunc;
        this.canExecute = canExecute ?? (_ => true);
    }

    #region Implementation of ICommand

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return canExecute(parameter);
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        executeFunc(parameter);
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    #endregion

    /// <summary>Raises <see cref="CanExecuteChanged"/> to re-query <see cref="CanExecute"/>.</summary>
    public void ChangeCanExecute()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
