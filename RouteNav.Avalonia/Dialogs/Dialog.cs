using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using RouteNav.Avalonia.Internal;

namespace RouteNav.Avalonia.Dialogs;

[PseudoClasses(SharedPseudoClasses.Hidden, SharedPseudoClasses.Open)]
public partial class Dialog : ContentControl
{
    protected Control? originalHost;
    protected int originalHostIndex;
    protected IInputElement? lastFocus;
    protected OverlayHost? overlayHost;
    protected TaskCompletionSource<object>? taskCompletionSource;

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

    private void OnOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Fired when the window is closed.
    /// </summary>
    public event EventHandler? Closed;

    private void OnClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Lifecycle

    public virtual Task<object> ShowDialog()
    {
        taskCompletionSource = new TaskCompletionSource<object>();

        if (Parent != null)
        {
            originalHost = (Control) Parent;
            switch (originalHost)
            {
                case Panel panel:
                    originalHostIndex = panel.Children.IndexOf(this);
                    panel.Children.Remove(this);
                    break;
                case Decorator decorator:
                    decorator.Child = null;
                    break;
                case ContentControl contentControl:
                    contentControl.Content = null;
                    break;
                case ContentPresenter contentPresenter:
                    contentPresenter.Content = null;
                    break;
            }
        }

        overlayHost ??= new OverlayHost();
        overlayHost.Content = this;

        var topLevel = ApplicationExtensions.GetTopLevel();
        lastFocus = topLevel.FocusManager?.GetFocusedElement();

        var overlayLayer = OverlayLayer.GetOverlayLayer(topLevel);
        if (overlayLayer == null)
            throw new InvalidOperationException();
        overlayLayer.Children.Add(overlayHost);

        IsVisible = true;
        overlayLayer.UpdateLayout();

        PseudoClasses.Set(SharedPseudoClasses.Hidden, false);
        PseudoClasses.Set(SharedPseudoClasses.Open, true);

        OnOpened();

        return taskCompletionSource.Task;
    }

    protected virtual async Task CloseDialog(object? result = null)
    {
        if (taskCompletionSource == null || overlayHost == null)
            return;

        IsHitTestVisible = false;

        Focus();

        PseudoClasses.Set(SharedPseudoClasses.Hidden, true);
        PseudoClasses.Set(SharedPseudoClasses.Open, false);

        await Task.Delay(200);

        OnClosed();

        if (lastFocus != null)
        {
            lastFocus.Focus();
            lastFocus = null;
        }

        var overlayLayer = OverlayLayer.GetOverlayLayer(overlayHost);
        if (overlayLayer == null)
            return;

        overlayLayer.Children.Remove(overlayHost);
        overlayHost.Content = null;

        if (originalHost != null)
        {
            switch (originalHost)
            {
                case Panel panel:
                    panel.Children.Insert(originalHostIndex, this);
                    break;
                case Decorator decorator:
                    decorator.Child = this;
                    break;
                case ContentControl contentControl:
                    contentControl.Content = this;
                    break;
                case ContentPresenter contentPresenter:
                    contentPresenter.Content = this;
                    break;
            }
        }

        taskCompletionSource.TrySetResult(result);
    }

    #endregion
}
