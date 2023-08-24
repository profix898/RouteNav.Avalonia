using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Media;
using RouteNav.Avalonia.Internal;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Dialogs;

[PseudoClasses(SharedPseudoClasses.Hidden, SharedPseudoClasses.Open, SharedPseudoClasses.DialogWindow)]
public partial class Dialog : ContentControl
{
    private TaskCompletionSource<object?>? taskCompletionSource;

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Dialog, string>(nameof(Title), "Dialog");

    public static readonly StyledProperty<Brush> TitleBarBackgroundProperty = AvaloniaProperty.Register<Dialog, Brush>(nameof(TitleBarBackground));

    public static readonly StyledProperty<Brush> TitleBarTextColorProperty = AvaloniaProperty.Register<Dialog, Brush>(nameof(TitleBarTextColor));

    public static readonly StyledProperty<Size> SizeProperty = AvaloniaProperty.Register<Dialog, Size>(nameof(Size));
    
    public Dialog()
    {
        PseudoClasses.Add(SharedPseudoClasses.Hidden);
    }

    public string Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public Brush TitleBarBackground
    {
        get { return GetValue(TitleBarBackgroundProperty); }
        set { SetValue(TitleBarBackgroundProperty, value); }
    }

    public Brush TitleBarTextColor
    {
        get { return GetValue(TitleBarTextColorProperty); }
        set { SetValue(TitleBarTextColorProperty, value); }
    }

    public Size Size
    {
        get { return GetValue(SizeProperty); }
        set { SetValue(SizeProperty, value); }
    }

    #region Events

    /// <summary>
    ///     Fired when the window is opened.
    /// </summary>
    public event EventHandler? Opened;

    protected internal virtual Task<object?> Open()
    {
        taskCompletionSource = new TaskCompletionSource<object?>();

        if (PlatformWindow == null) // Shown in overlay display
        {
            PseudoClasses.Set(SharedPseudoClasses.Hidden, false);
            PseudoClasses.Set(SharedPseudoClasses.Open, true);

            Opened?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            PseudoClasses.Set(SharedPseudoClasses.DialogWindow, true);
            PseudoClasses.Set(SharedPseudoClasses.Hidden, false);
            PseudoClasses.Set(SharedPseudoClasses.Open, true);
        }

        return taskCompletionSource.Task;
    }

    /// <summary>
    ///     Fired when the window is closed.
    /// </summary>
    public event EventHandler? Closed;

    public virtual void Close(object? result = null)
    {
        if (taskCompletionSource == null)
            return;

        Result = result;
        taskCompletionSource.TrySetResult(result);

        if (PlatformWindow == null) // Shown in overlay display
        {
            IsHitTestVisible = false;
            Focus();

            PseudoClasses.Set(SharedPseudoClasses.Hidden, true);
            PseudoClasses.Set(SharedPseudoClasses.Open, false);

            // Animation delay
            Task.Delay(200).ContinueWith(_ => Closed?.Invoke(this, EventArgs.Empty));
        }
        else
        {
            PlatformWindow.Close(result);
        }
    }

    #endregion

    #region Result

    [MemberNotNullWhen(true, nameof(ResultTask))]
    public bool IsOpen => taskCompletionSource != null;

    internal Task<object?>? ResultTask => taskCompletionSource?.Task;

    public object? Result { get; private set; }

    #endregion

    #region Platform

    /// <summary>
    /// Gets the associated platform window
    /// </summary>
    public AvaloniaWindow? PlatformWindow { get; private set; }

    internal void RegisterPlatform(AvaloniaWindow platformWindow)
    {
        PlatformWindow = platformWindow;

        platformWindow.Opened += (_, _) => Opened?.Invoke(this, EventArgs.Empty);
        platformWindow.Closed += (_, _) =>
        {
            if (taskCompletionSource == null)
                return; // Dialog was never opened

            if (!taskCompletionSource.Task.IsCompleted)
            {
                // Dialog window closed directly (set result and finalize task)
                Result = null;
                taskCompletionSource.TrySetResult(null);
            }

            Closed?.Invoke(this, EventArgs.Empty);
        };
    }

    #endregion
}
