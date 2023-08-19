using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.StackControls;

public class NavigationContainer : ContentControl, ISafeAreaAware
{
    private TopLevel? topLevel;
    private IInsetsManager? insetsManager;
    private INavigationStack? navigationStack;

    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<Page, Thickness>(nameof(SafeAreaPadding));

    protected override Type StyleKeyOverride => typeof(ContentControl);

    public INavigationStack? NavigationStack
    {
        get { return navigationStack; }
        internal init
        {
            if (navigationStack != null)
                navigationStack.PageNavigated -= OnPageNavigated;
            navigationStack = value;
            if (navigationStack != null)
                navigationStack.PageNavigated += OnPageNavigated;
        }
    }

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
            if (Presenter.Child is ISafeAreaAware safeAreaAware)
                safeAreaAware.SafeAreaPadding = remainingSafeArea;
            else
                Presenter.Padding = Presenter.Padding.ApplySafeAreaPadding(remainingSafeArea);
        }
    }

    protected virtual void OnPageNavigated((Page? pageFrom, Page? pageTo) e)
    {
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
