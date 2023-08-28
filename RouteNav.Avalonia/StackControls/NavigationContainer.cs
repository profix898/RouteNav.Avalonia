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
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.StackControls;

public class NavigationContainer : ContentControl, ISafeAreaAware
{
    private TopLevel? topLevel;
    private IInsetsManager? insetsManager;
    private INavigationStack? navigationStack;
    private DialogOverlayHost? dialogOverlayHost;

    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<Page, Thickness>(nameof(SafeAreaPadding));

    #region NavigationContainer

    public INavigationStack? NavigationStack
    {
        get { return navigationStack; }
        internal set { navigationStack = value; }
    }

    public DialogOverlayHost DialogOverlayHost
    {
        get
        {
            if (dialogOverlayHost == null)
            {
                if (topLevel == null || navigationStack == null)
                    throw new InvalidOperationException();
                var overlayLayer = OverlayLayer.GetOverlayLayer(topLevel);
                if (overlayLayer == null)
                    throw new InvalidOperationException();

                dialogOverlayHost = overlayLayer.Children.OfType<DialogOverlayHost>().FirstOrDefault()
                                    ?? new DialogOverlayHost(topLevel, overlayLayer);
            }

            return dialogOverlayHost;
        }
    }

    protected override Type StyleKeyOverride => typeof(ContentControl);

    public virtual void UpdatePage(Page? page)
    {
        Content = page?.Content;
    }

    public virtual Task<object?> UpdateDialog(Dialog? dialog)
    {
        // All dialogs closed (dispose overlay host)
        if (dialog == null)
        {
            dialogOverlayHost?.Dispose();

            return Task.FromResult<object?>(null);
        }

        // Dialog already open (update content and return task)
        if (dialog.IsOpen)
        {
            if (!Navigation.UIPlatform.WindowManager.SupportsMultiWindow
                || Navigation.UIPlatform.WindowManager.ForceOverlayDialogs)
            {
                DialogOverlayHost.Content = dialog;
                DialogOverlayHost.OverlayLayer.UpdateLayout();
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
            && !Navigation.UIPlatform.WindowManager.ForceOverlayDialogs)
        {
            // Open new dialog in window
            var parentWindow = Navigation.UIPlatform.GetActiveWindowFromStack(navigationStack);
            if (Navigation.UIPlatform.WindowManager.OpenDialog(dialog, out var dialogTask, parentWindow))
                return dialogTask;
        }

        DialogOverlayHost.Content = dialog;
        dialog.IsVisible = true;
        DialogOverlayHost.OverlayLayer.UpdateLayout();

        return dialog.Open();
    }

    #endregion

    #region Implementation of ISafeAreaAware

    public Thickness SafeAreaPadding
    {
        get { return GetValue(SafeAreaPaddingProperty); }
        set { SetValue(SafeAreaPaddingProperty, value); }
    }

    #endregion

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (topLevel != null)
            topLevel.BackRequested -= SystemBackRequested;
        if (insetsManager != null)
            insetsManager.SafeAreaChanged -= SafeAreaChanged;

        topLevel = TopLevel.GetTopLevel(this);
        insetsManager = topLevel?.InsetsManager;

        if (topLevel != null)
            topLevel.BackRequested += SystemBackRequested;
        if (insetsManager != null)
        {
            SafeAreaPadding = insetsManager.SafeAreaPadding;
            insetsManager.SafeAreaChanged += SafeAreaChanged;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UpdateContentSafeAreaPadding();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SafeAreaPaddingProperty || change.Property == PaddingProperty || change.Property == ContentProperty)
            UpdateContentSafeAreaPadding();
    }

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
}
