using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Platform;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.StackContainers;

/// <summary>Base container that hosts a navigation stack's current page and dialog overlays.</summary>
public class NavigationContainer : ContentControl, ISafeAreaAware
{
    private TopLevel? topLevel;
    private IInsetsManager? insetsManager;

    /// <summary>Defines the <see cref="SafeAreaPadding" /> property.</summary>
    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<NavigationContainer, Thickness>(nameof(SafeAreaPadding));

    #region NavigationContainer

    /// <summary>Raised when the host control used by the container is attached/resolved.</summary>
    public event Action? HostControlAttached;

    /// <summary>Raises <see cref="HostControlAttached" />.</summary>
    protected void OnHostControlAttached()
    {
        HostControlAttached?.Invoke();
    }

    /// <summary>Gets the navigation stack associated with this container.</summary>
    public INavigationStack? NavigationStack { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Dialogs.DialogOverlayHost" /> for the current top level, creating and attaching
    /// it to the overlay layer on first use.
    /// </summary>
    /// <remarks>This is a factory-style accessor and has a side effect on first call (it creates the host).</remarks>
    public DialogOverlayHost GetDialogOverlayHost()
    {
        if (topLevel == null || NavigationStack == null)
            throw new InvalidOperationException("NavigationContainer is not attached to a TopLevel yet.");

        var overlayLayer = OverlayLayer.GetOverlayLayer(topLevel)
                           ?? throw new InvalidOperationException("No OverlayLayer available on the current TopLevel.");

        return overlayLayer.Children.OfType<DialogOverlayHost>().FirstOrDefault()
               ?? new DialogOverlayHost(topLevel, overlayLayer);
    }

    /// <summary>Updates the page shown by the container.</summary>
    public virtual void UpdatePage(Page? page)
    {
        Content = page?.Content;
    }

    /// <summary>Updates the dialog shown by the container, opening it in a window or overlay as appropriate.</summary>
    public virtual Task<object?> UpdateDialog(Dialog? dialog, bool forceOverlay = false)
    {
        // All dialogs closed (dispose overlay host)
        if (dialog == null)
        {
            //dialogOverlayHost?.Dispose();

            return Task.FromResult<object?>(null);
        }

        // Dialog already open (update content and return task)
        if (dialog.IsOpen)
        {
            if (!Navigation.UIPlatform.WindowManager.SupportsMultiWindow
                || Navigation.UIPlatform.WindowManager.ForceOverlayDialogs || forceOverlay)
            {
                // Remove dialog size (so that background fills host container)
                dialog.Width = dialog.Height = Double.NaN;

                // Update content of dialog host
                GetDialogOverlayHost().SetContent(dialog);
            }

            return dialog.ResultTask;
        }

        // Open new dialog (open in dialog window or overlay)
        if (dialog.Parent != null)
        {
            // Isolate dialog from associated parent (if any)
            switch ((Control) dialog.Parent)
            {
                case Panel panel:
                    panel.Children.Remove(dialog);
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

        if (Navigation.UIPlatform.WindowManager.SupportsMultiWindow
            && !Navigation.UIPlatform.WindowManager.ForceOverlayDialogs && !forceOverlay)
        {
            // Open new dialog in window
            var parentWindow = Navigation.UIPlatform.GetActiveWindowFromStack(NavigationStack);
            if (Navigation.UIPlatform.WindowManager.OpenDialog(dialog, out var dialogTask, parentWindow))
                return dialogTask;
        }

        // Remove dialog size (so that background fills host container)
        dialog.Width = dialog.Height = Double.NaN;

        // Update content of dialog host
        GetDialogOverlayHost().SetContent(dialog);
        dialog.IsVisible = true;

        return dialog.Open();
    }

    #endregion

    #region Implementation of ISafeAreaAware

    /// <inheritdoc />
    public Thickness SafeAreaPadding
    {
        get { return GetValue(SafeAreaPaddingProperty); }
        set { SetValue(SafeAreaPaddingProperty, value); }
    }

    #endregion

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ContentControl);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (topLevel != null)
            topLevel.BackRequested -= SystemBackRequested;
        if (insetsManager != null)
            insetsManager.SafeAreaChanged -= SafeAreaChanged;

        topLevel = TopLevel.GetTopLevel(this)
                   ?? throw new InvalidOperationException("No TopLevel found.");
        insetsManager = topLevel.InsetsManager;

        topLevel.BackRequested += SystemBackRequested;
        if (insetsManager != null)
        {
            SafeAreaPadding = insetsManager.SafeAreaPadding;
            insetsManager.SafeAreaChanged += SafeAreaChanged;
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UpdateContentSafeAreaPadding();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SafeAreaPaddingProperty || change.Property == PaddingProperty || change.Property == ContentProperty)
            UpdateContentSafeAreaPadding();
    }

    /// <summary>Applies the remaining safe-area padding to the hosted content.</summary>
    protected virtual void UpdateContentSafeAreaPadding()
    {
        if (Content != null && Presenter != null)
        {
            var remainingSafeArea = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);

            if (Presenter.Child is ISafeAreaAware safeAreaAwareChild)
                safeAreaAwareChild.SafeAreaPadding = remainingSafeArea;
            else
                Presenter.Padding = Presenter.Padding.ApplySafeAreaPadding(remainingSafeArea);
        }
    }

    #region EventHandlers

    private void SystemBackRequested(object? sender, RoutedEventArgs e)
    {
        Navigation.PopAsync(NavigationStack);

        e.Handled = true;
    }

    private void SafeAreaChanged(object? sender, SafeAreaChangedArgs e)
    {
        SafeAreaPadding = e.SafeAreaPadding;
    }

    #endregion

    #region Internal

    /// <summary>Registers a named control in a scoped name scope on the given element.</summary>
    protected static void RegisterScopedControl(StyledElement styledElement, string name, object element)
    {
        var nameScope = new NameScope();
        NameScope.SetNameScope(styledElement, nameScope);
        nameScope.Register(name, element);
    }

    #endregion
}
