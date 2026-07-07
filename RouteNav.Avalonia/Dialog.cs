using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Stacks;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia;

/// <summary>
/// A dialog surface that can be shown as a native window (desktop) or an in-app overlay/embedded control
/// (mobile/browser), returning a result via <see cref="ShowDialog(Window?, bool)"/>.
/// </summary>
[PseudoClasses(SharedPseudoClasses.Hidden, SharedPseudoClasses.Open, SharedPseudoClasses.DialogWindow, SharedPseudoClasses.DialogEmbedded)]
public class Dialog : ContentControl
{
    /// <summary>Completion source used by the active dialog result task.</summary>
    protected TaskCompletionSource<object?>? taskCompletionSource;
    /// <summary>The close button resolved from the control template, if present.</summary>
    protected Button? dialogCloseButton;
    /// <summary>The title bar panel resolved from the control template, if present.</summary>
    protected Panel? dialogTitleBarPanel;

    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Dialog, string>(nameof(Title), "Dialog");

    /// <summary>
    /// Defines the <see cref="TitleBarBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<Brush> TitleBarBackgroundProperty = AvaloniaProperty.Register<Dialog, Brush>(nameof(TitleBarBackground));

    /// <summary>
    /// Defines the <see cref="TitleBarTextColor"/> property.
    /// </summary>
    public static readonly StyledProperty<Brush> TitleBarTextColorProperty = AvaloniaProperty.Register<Dialog, Brush>(nameof(TitleBarTextColor));
    
    /// <summary>
    /// Defines the <see cref="DialogSize"/> property.
    /// </summary>
    public static readonly StyledProperty<DialogSize> DialogSizeProperty = AvaloniaProperty.Register<Dialog, DialogSize>(nameof(DialogSize), DialogSize.Medium);

    /// <summary>Initializes a new instance of the <see cref="Dialog"/> class.</summary>
    public Dialog()
    {
        PseudoClasses.Add(SharedPseudoClasses.Hidden);

        if (Design.IsDesignMode)
        {
            // Design-time view as embedded dialog
            PseudoClasses.Set(SharedPseudoClasses.Hidden, false);
            PseudoClasses.Set(SharedPseudoClasses.DialogEmbedded, true);
            
        }
    }

    /// <summary>
    /// Gets or sets the title of the dialog
    /// </summary>
    public string Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    /// <summary>
    /// Gets or sets the background color of the dialog's title bar
    /// </summary>
    public Brush TitleBarBackground
    {
        get { return GetValue(TitleBarBackgroundProperty); }
        set { SetValue(TitleBarBackgroundProperty, value); }
    }

    /// <summary>
    /// Gets or sets the text color of the dialog's title
    /// </summary>
    public Brush TitleBarTextColor
    {
        get { return GetValue(TitleBarTextColorProperty); }
        set { SetValue(TitleBarTextColorProperty, value); }
    }

    /// <summary>
    /// Gets or sets a size specifier for the dialog
    /// </summary>
    public DialogSize DialogSize
    {
        get { return GetValue(DialogSizeProperty); }
        set { SetValue(DialogSizeProperty, value); }
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Dialog);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (dialogCloseButton != null)
            dialogCloseButton.Click -= CloseDialog;
        dialogCloseButton = e.NameScope.Find<Button>("DialogCloseButton");
        if (dialogCloseButton != null) // MessageDialog does not have a close button
            dialogCloseButton.Click += CloseDialog;
        
        dialogTitleBarPanel = e.NameScope.Find<Panel>("DialogTitleBar");
        
        // Correct page/dialog size (to account for page margins and title bar height)
        if (Content is Page page)
        {
            var titleBarHeight = dialogTitleBarPanel?.Height ?? 0;
            Height += titleBarHeight;
            page.Bind(HeightProperty, this.GetBindingObservable(HeightProperty, height => height - titleBarHeight));
            page.Bind(WidthProperty, this.GetBindingObservable(WidthProperty));
        }
    }

    #region Events

    /// <summary>Fired when the window is opened.</summary>
    public event EventHandler? Opened;

    /// <summary>Opens the dialog and returns the task completed when it closes.</summary>
    protected internal virtual Task<object?> Open()
    {
        taskCompletionSource = new TaskCompletionSource<object?>();

        if (PlatformWindow == null) // Shown in overlay display
        {
            PseudoClasses.Set(SharedPseudoClasses.DialogWindow, false);
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

    /// <summary>Fired when the window is closed.</summary>
    public event EventHandler? Closed;

    /// <summary>Closes the dialog, completing its result task with <paramref name="result"/>.</summary>
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
            Task.Delay(180).ContinueWith(_ => Closed?.Invoke(this, EventArgs.Empty));
        }
        else
        {
            PlatformWindow.Close(result);
        }
    }

    #endregion

    #region Result

    private void CloseDialog(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>Gets a value indicating whether the dialog is currently open.</summary>
    [MemberNotNullWhen(true, nameof(ResultTask))]
    public bool IsOpen => taskCompletionSource != null;

    internal Task<object?>? ResultTask => taskCompletionSource?.Task;

    /// <summary>Gets the result the dialog was closed with, if any.</summary>
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

    #region ShowDialog

    /// <summary>Shows the dialog on the stack associated with the given window (or the main window).</summary>
    public Task<object?> ShowDialog(Window? parentWindow = null, bool forceOverlay = false)
    {
        var stack = Navigation.UIPlatform.GetActiveStackFromWindow(parentWindow) ?? Navigation.GetMainStack();
        
        return stack.PushDialogAsync(this, forceOverlay);
    }

    /// <summary>Shows the dialog on the stack that owns the given page (or the main stack).</summary>
    public Task<object?> ShowDialog(Page? parentPage, bool forceOverlay = false)
    {
        INavigationStack? stack = null;
        if (parentPage != null && parentPage.PageQuery.TryGetValue("routeUri", out var routeUriString))
        {
            var stackName = new Uri(routeUriString).GetStackName();
            if (!String.IsNullOrEmpty(stackName))
                stack = Navigation.UIPlatform.GetStack(stackName);
        }
        stack ??= Navigation.GetMainStack();
    
        return stack.PushDialogAsync(this, forceOverlay);
    }

    /// <summary>Shows the dialog embedded inside the given content control, optionally restoring its previous content on close.</summary>
    public Task<object?> ShowDialogEmbedded(ContentControl parentControl, bool restoreParent = false)
    {
        var previousContent = parentControl.Content;
        
        this.SetSizeBinding(parentControl);

        parentControl.Content = this;
        PseudoClasses.Set(SharedPseudoClasses.DialogEmbedded, true);

        var dialogTask = Open();
        if (restoreParent)
            dialogTask.ContinueWith(_ => { Dispatcher.UIThread.Invoke(() => parentControl.Content = previousContent); });

        return dialogTask;
    }

    #endregion
}
