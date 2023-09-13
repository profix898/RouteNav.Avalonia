using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Stacks;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Dialogs;

[TemplatePart("PART_DialogCloseButton", typeof(Button))]
[TemplatePart("PART_DialogContent", typeof(ContentPresenter))]
[PseudoClasses(SharedPseudoClasses.Hidden, SharedPseudoClasses.Open, SharedPseudoClasses.DialogWindow, SharedPseudoClasses.DialogEmbedded)]
public class Dialog : TemplatedControl
{
    protected TaskCompletionSource<object?>? taskCompletionSource;
    protected Button? dialogCloseButton;
    protected ContentPresenter? dialogContent;

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Dialog, string>(nameof(Title), "Dialog");

    public static readonly StyledProperty<Brush> TitleBarBackgroundProperty = AvaloniaProperty.Register<Dialog, Brush>(nameof(TitleBarBackground));

    public static readonly StyledProperty<Brush> TitleBarTextColorProperty = AvaloniaProperty.Register<Dialog, Brush>(nameof(TitleBarTextColor));

    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<Dialog, object?>(nameof(Content));

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty = AvaloniaProperty.Register<TemplatedControl, IDataTemplate?>(nameof(ContentTemplate));

    public static readonly StyledProperty<DialogSize> DialogSizeProperty = AvaloniaProperty.Register<Dialog, DialogSize>(nameof(DialogSize), DialogSize.Medium);

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

    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get { return GetValue(ContentProperty); }
        set
        {
            if (value is string strValue)
                value = new TextBlock { Text = strValue };

            SetValue(ContentProperty, value);
        }
    }

    public IDataTemplate? ContentTemplate
    {
        get { return GetValue(ContentTemplateProperty); }
        set { SetValue(ContentTemplateProperty, value); }
    }

    public DialogSize DialogSize
    {
        get { return GetValue(DialogSizeProperty); }
        set { SetValue(DialogSizeProperty, value); }
    }

    protected override Type StyleKeyOverride => typeof(Dialog);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (dialogCloseButton != null)
            dialogCloseButton.Click -= CloseDialog;
        dialogCloseButton = e.NameScope.Find<Button>("PART_DialogCloseButton");
        if (dialogCloseButton != null) // MessageDialog does not have a close button
            dialogCloseButton.Click += CloseDialog;

        if (dialogContent != null)
            dialogContent.PropertyChanged -= ContentPresenter_ChildPropertyChanged;
        dialogContent = e.NameScope.Get<ContentPresenter>("PART_DialogContent");
        dialogContent.PropertyChanged += ContentPresenter_ChildPropertyChanged;

        dialogContent.Content = Content;
    }

    private void ContentPresenter_ChildPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
        {
            if (e.OldValue is ILogical oldChild)
                LogicalChildren.Remove(oldChild);
            if (e.NewValue is ILogical newLogical)
                LogicalChildren.Add(newLogical);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty && dialogContent != null)
            dialogContent.Content = Content;
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

    private void CloseDialog(object? sender, RoutedEventArgs e)
    {
        Close();
    }

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

    #region ShowDialog

    public Task<object?> ShowDialog(Window? parentWindow = null)
    {
        var stack = Navigation.UIPlatform.GetActiveStackFromWindow(parentWindow) ?? Navigation.GetMainStack();
        
        return stack.PushDialogAsync(this);
    }

    public Task<object?> ShowDialog(Page? parentPage)
    {
        INavigationStack? stack = null;
        if (parentPage != null && parentPage.PageQuery.TryGetValue("routeUri", out var routeUriString))
        {
            var stackName = new Uri(routeUriString).GetStackName();
            if (!String.IsNullOrEmpty(stackName))
                stack = Navigation.UIPlatform.GetStack(stackName);
        }
        stack ??= Navigation.GetMainStack();
    
        return stack.PushDialogAsync(this);
    }

    public Task<object?> ShowDialogEmbedded(ContentControl parentControl, bool restoreParent = false)
    {
        var previousContent = parentControl.Content;

        parentControl.Content = this;
        PseudoClasses.Set(SharedPseudoClasses.DialogEmbedded, true);

        var dialogTask = Open();
        if (restoreParent)
            dialogTask.ContinueWith(_ => { Dispatcher.UIThread.Invoke(() => parentControl.Content = previousContent); });

        return dialogTask;
    }

    #endregion
}
