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

    public static readonly StyledProperty<IBrush> NavigationBarBackgroundProperty = AvaloniaProperty.Register<NavigationControl, IBrush>(nameof(NavigationBarBackground));

    public static readonly StyledProperty<IBrush> NavigationBarTextColorProperty = AvaloniaProperty.Register<NavigationControl, IBrush>(nameof(NavigationBarTextColor));

    public static readonly StyledProperty<bool> NavigationBarVisibleProperty = AvaloniaProperty.Register<NavigationControl, bool>(nameof(NavigationBarVisible), true);

    public static readonly StyledProperty<bool> BackButtonEnabledProperty = AvaloniaProperty.Register<NavigationControl, bool>(nameof(BackButtonEnabled));

    public static readonly RoutedEvent<RoutedEventArgs> BackButtonClickEvent = RoutedEvent.Register<NavigationControl, RoutedEventArgs>(nameof(BackButtonClick), RoutingStrategies.Bubble);

    public static readonly StyledProperty<Page?> PageProperty = AvaloniaProperty.Register<NavigationControl, Page?>(nameof(Page));

    public static readonly StyledProperty<IPageTransition> PageTransitionProperty = AvaloniaProperty.Register<NavigationControl, IPageTransition>(nameof(PageTransition), new CrossFade(TimeSpan.FromSeconds(0.125)));

    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<NavigationControl, Thickness>(nameof(SafeAreaPadding));

    public IBrush? NavigationBarBackground
    {
        get { return GetValue(NavigationBarBackgroundProperty); }
        set { SetValue(NavigationBarBackgroundProperty, value); }
    }

    public IBrush? NavigationBarTextColor
    {
        get { return GetValue(NavigationBarTextColorProperty); }
        set { SetValue(NavigationBarTextColorProperty, value); }
    }

    public bool NavigationBarVisible
    {
        get { return GetValue(NavigationBarVisibleProperty); }
        set { SetValue(NavigationBarVisibleProperty, value); }
    }

    public bool BackButtonEnabled
    {
        get { return GetValue(BackButtonEnabledProperty); }
        set { SetValue(BackButtonEnabledProperty, value); }
    }

    public event EventHandler<RoutedEventArgs>? BackButtonClick
    {
        add => AddHandler(BackButtonClickEvent, value);
        remove => RemoveHandler(BackButtonClickEvent, value);
    }

    public Page? Page
    {
        get { return GetValue(PageProperty); }
        set { SetValue(PageProperty, value); }
    }

    public IPageTransition PageTransition
    {
        get { return GetValue(PageTransitionProperty); }
        set { SetValue(PageTransitionProperty, value); }
    }

    #region Implementation of ISafeAreaAware

    public Thickness SafeAreaPadding
    {
        get { return GetValue(SafeAreaPaddingProperty); }
        set { SetValue(SafeAreaPaddingProperty, value); }
    }

    #endregion

    protected override Type StyleKeyOverride => typeof(NavigationControl);

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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if ((change.Property == PageTransitionProperty || change.Property == PageProperty) && navContentControl != null)
            UpdateContent();
        else if (change.Property == PageTransitionProperty && navContentControl != null)
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
