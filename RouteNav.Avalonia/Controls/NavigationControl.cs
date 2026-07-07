using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Platform;

namespace RouteNav.Avalonia.Controls;

/// <summary>A navigation chrome control that shows a page with a title bar, back button and page transition.</summary>
[TemplatePart("PART_NavigationBar", typeof(Border))]
[TemplatePart("PART_NavigationBarBackButton", typeof(Button))]
[TemplatePart("PART_NavigationBarTitle", typeof(ContentPresenter))]
[TemplatePart("PART_NavigationContent", typeof(TransitioningContentControl))]
public sealed class NavigationControl : TemplatedControl, ISafeAreaAware
{
    private Border? navBarBorder;
    private Button? navBarBackButton;
    private ContentPresenter? navBarTitle;
    private TransitioningContentControl? navContentControl;

    /// <summary>Defines the <see cref="NavigationBarBackground"/> property.</summary>
    public static readonly StyledProperty<IBrush> NavigationBarBackgroundProperty = AvaloniaProperty.Register<NavigationControl, IBrush>(nameof(NavigationBarBackground));

    /// <summary>Defines the <see cref="NavigationBarTextColor"/> property.</summary>
    public static readonly StyledProperty<IBrush> NavigationBarTextColorProperty = AvaloniaProperty.Register<NavigationControl, IBrush>(nameof(NavigationBarTextColor));

    /// <summary>Defines the <see cref="NavigationBarVisible"/> property.</summary>
    public static readonly StyledProperty<bool> NavigationBarVisibleProperty = AvaloniaProperty.Register<NavigationControl, bool>(nameof(NavigationBarVisible), true);

    /// <summary>Defines the <see cref="BackButtonEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> BackButtonEnabledProperty = AvaloniaProperty.Register<NavigationControl, bool>(nameof(BackButtonEnabled));

    /// <summary>Defines the <see cref="BackButtonClick"/> routed event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> BackButtonClickEvent = RoutedEvent.Register<NavigationControl, RoutedEventArgs>(nameof(BackButtonClick), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="Page"/> property.</summary>
    public static readonly StyledProperty<Page?> PageProperty = AvaloniaProperty.Register<NavigationControl, Page?>(nameof(Page));

    /// <summary>Defines the <see cref="PageTransition"/> property.</summary>
    public static readonly StyledProperty<IPageTransition> PageTransitionProperty = AvaloniaProperty.Register<NavigationControl, IPageTransition>(nameof(PageTransition), new CrossFade(TimeSpan.FromSeconds(0.125)));

    /// <summary>Defines the <see cref="SafeAreaPadding"/> property.</summary>
    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<NavigationControl, Thickness>(nameof(SafeAreaPadding));

    /// <summary>Gets or sets the background brush of the navigation bar.</summary>
    public IBrush? NavigationBarBackground
    {
        get { return GetValue(NavigationBarBackgroundProperty); }
        set { SetValue(NavigationBarBackgroundProperty, value); }
    }

    /// <summary>Gets or sets the foreground (text) brush of the navigation bar.</summary>
    public IBrush? NavigationBarTextColor
    {
        get { return GetValue(NavigationBarTextColorProperty); }
        set { SetValue(NavigationBarTextColorProperty, value); }
    }

    /// <summary>Gets or sets a value indicating whether the navigation bar is visible.</summary>
    public bool NavigationBarVisible
    {
        get { return GetValue(NavigationBarVisibleProperty); }
        set { SetValue(NavigationBarVisibleProperty, value); }
    }

    /// <summary>Gets or sets a value indicating whether the back button is enabled.</summary>
    public bool BackButtonEnabled
    {
        get { return GetValue(BackButtonEnabledProperty); }
        set { SetValue(BackButtonEnabledProperty, value); }
    }

    /// <summary>Raised when the navigation bar back button is clicked (while enabled).</summary>
    public event EventHandler<RoutedEventArgs>? BackButtonClick
    {
        add => AddHandler(BackButtonClickEvent, value);
        remove => RemoveHandler(BackButtonClickEvent, value);
    }

    /// <summary>Gets or sets the page currently hosted by this navigation control.</summary>
    public Page? Page
    {
        get { return GetValue(PageProperty); }
        set { SetValue(PageProperty, value); }
    }

    /// <summary>Gets or sets the transition played when the hosted page changes.</summary>
    public IPageTransition PageTransition
    {
        get { return GetValue(PageTransitionProperty); }
        set { SetValue(PageTransitionProperty, value); }
    }

    #region Implementation of ISafeAreaAware

    /// <inheritdoc />
    public Thickness SafeAreaPadding
    {
        get { return GetValue(SafeAreaPaddingProperty); }
        set { SetValue(SafeAreaPaddingProperty, value); }
    }

    #endregion

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(NavigationControl);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        navBarBorder = e.NameScope.Get<Border>("PART_NavigationBar");

        if (navBarBackButton != null)
            navBarBackButton.Click -= BackButton_Clicked;
        navBarBackButton = e.NameScope.Get<Button>("PART_NavigationBarBackButton");
        if (navBarBackButton != null)
            navBarBackButton.Click += BackButton_Clicked;

        if (navBarTitle != null)
            navBarTitle.PropertyChanged -= NavContentPresenter_PropertyChanged;
        navBarTitle = e.NameScope.Get<ContentPresenter>("PART_NavigationBarTitle");
        navBarTitle.PropertyChanged += NavContentPresenter_PropertyChanged;

        if (navContentControl != null)
            navContentControl.PropertyChanged -= NavContentPresenter_PropertyChanged;
        navContentControl = e.NameScope.Get<TransitioningContentControl>("PART_NavigationContent");
        navContentControl.PageTransition = PageTransition;
        navContentControl.PropertyChanged += NavContentPresenter_PropertyChanged;

        UpdateContent();
    }

    private void NavContentPresenter_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty || e.Property == ContentControl.ContentProperty)
        {
            if (e.OldValue is ILogical oldChild)
                LogicalChildren.Remove(oldChild);
            if (e.NewValue is ILogical newLogical)
                LogicalChildren.Add(newLogical);
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (navContentControl == null)
            return;

        if (change.Property == PageProperty)
            UpdateContent();
        else if (change.Property == PageTransitionProperty)
            navContentControl.PageTransition = PageTransition;
    }

    private void UpdateContent()
    {
        navBarTitle?.SetValue(ContentPresenter.ContentProperty, Page?.Title ?? String.Empty);
        navContentControl?.SetValue(ContentControl.ContentProperty, Page);

        UpdateContentSafeAreaPadding();
    }

    private void UpdateContentSafeAreaPadding()
    {
        var remainingSafeArea = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);

        if (navBarBorder != null)
        {
            navBarBorder.Padding = navBarBorder.Padding.ApplySafeAreaPadding(new Thickness(SafeAreaPadding.Left, SafeAreaPadding.Top, SafeAreaPadding.Right, 0));
            remainingSafeArea = navBarBorder.Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);
        }

        if (navContentControl != null)
        {
            if (navContentControl.Content is ISafeAreaAware safeAreaAwareChild)
                safeAreaAwareChild.SafeAreaPadding = remainingSafeArea;
            else
                navContentControl.Padding = navContentControl.Padding.ApplySafeAreaPadding(remainingSafeArea);
        }
    }

    private void BackButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (BackButtonEnabled)
            RaiseEvent(new RoutedEventArgs(BackButtonClickEvent));
    }
}
