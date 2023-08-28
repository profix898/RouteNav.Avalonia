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

namespace RouteNav.Avalonia.StackControls;

[TemplatePart("PART_NavigationBar", typeof(Border))]
[TemplatePart("PART_NavigationBarBackButton", typeof(Button))]
[TemplatePart("PART_NavigationBarTitle", typeof(ContentPresenter))]
[TemplatePart("PART_NavigationContent", typeof(TransitioningContentControl))]
public sealed class NavigationPageContainer : NavigationContainer
{
    private Border? navBarBorder;
    private Button? navBarBackButton;
    private ContentPresenter? navBarTitle;
    private TransitioningContentControl? navContentControl;

    public static readonly StyledProperty<IBrush> NavBarBackgroundProperty = AvaloniaProperty.Register<NavigationPageContainer, IBrush>(nameof(NavBarBackground));

    public static readonly StyledProperty<IBrush> NavBarTextColorProperty = AvaloniaProperty.Register<NavigationPageContainer, IBrush>(nameof(NavBarTextColor));

    public static readonly StyledProperty<bool> NavBarVisibleProperty = AvaloniaProperty.Register<NavigationPageContainer, bool>(nameof(NavBarVisible), true);

    public static readonly StyledProperty<IPageTransition> PageTransitionProperty =
        AvaloniaProperty.Register<NavigationPageContainer, IPageTransition>(nameof(PageTransition), new CrossFade(TimeSpan.FromSeconds(0.125)));

    public static readonly DirectProperty<NavigationPageContainer, bool?> CanGoBackProperty =
        AvaloniaProperty.RegisterDirect<NavigationPageContainer, bool?>(nameof(CanGoBack), o => o.CanGoBack);

    public IBrush? NavBarBackground
    {
        get { return GetValue(NavBarBackgroundProperty); }
        set { SetValue(NavBarBackgroundProperty, value); }
    }

    public IBrush? NavBarTextColor
    {
        get { return GetValue(NavBarTextColorProperty); }
        set { SetValue(NavBarTextColorProperty, value); }
    }

    public bool NavBarVisible
    {
        get { return GetValue(NavBarVisibleProperty); }
        set { SetValue(NavBarVisibleProperty, value); }
    }

    public IPageTransition PageTransition
    {
        get { return GetValue(PageTransitionProperty); }
        set { SetValue(PageTransitionProperty, value); }
    }

    protected override Type StyleKeyOverride => typeof(NavigationPageContainer);

    #region InternalNavigation

    public bool CanGoBack
    {
        get
        {
            if (NavigationStack == null)
                return false;

            return NavigationStack.PageStack.Count > (NavigationStack.IsMainStack ? 1 : 0);
        }
    }

    public Page CurrentPage => NavigationStack?.CurrentPage ?? new Page();

    private void NavBarBackButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (CanGoBack)
            Navigation.PopAsync(NavigationStack);
    }

    #endregion

    public override void UpdatePage(Page page)
    {
        // Title
        navBarTitle?.SetValue(ContentPresenter.ContentProperty, page.Title);

        // Content
        navContentControl?.SetValue(ContentProperty, page);
        RaisePropertyChanged(CanGoBackProperty, null, CanGoBack);

        UpdateContentSafeAreaPadding();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        navBarBorder = e.NameScope.Get<Border>("PART_NavigationBar");

        if (navBarBackButton != null)
            navBarBackButton.Click -= NavBarBackButton_Clicked;
        navBarBackButton = e.NameScope.Get<Button>("PART_NavigationBarBackButton");
        if (navBarBackButton != null)
            navBarBackButton.Click += NavBarBackButton_Clicked;

        if (navBarTitle != null)
            navBarTitle.PropertyChanged -= ContentPresenter_ChildPropertyChanged;
        navBarTitle = e.NameScope.Get<ContentPresenter>("PART_NavigationBarTitle");
        navBarTitle.PropertyChanged += ContentPresenter_ChildPropertyChanged;

        if (navContentControl != null)
        {
            navContentControl.TemplateApplied -= NavContentControl_OnTemplateApplied;
            if (navContentControl?.Presenter != null)
                navContentControl.Presenter.PropertyChanged -= ContentPresenter_ChildPropertyChanged;
        }
        navContentControl = e.NameScope.Get<TransitioningContentControl>("PART_NavigationContent");
        navContentControl.PageTransition = PageTransition;
        navContentControl.TemplateApplied += NavContentControl_OnTemplateApplied;

        UpdatePage(CurrentPage);
    }

    private void NavContentControl_OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (navContentControl?.Presenter != null)
            navContentControl.Presenter.PropertyChanged += ContentPresenter_ChildPropertyChanged;
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

        if (change.Property == PageTransitionProperty && navContentControl != null)
            navContentControl.PageTransition = PageTransition;
    }

    protected override void UpdateContentSafeAreaPadding()
    {
        if (navContentControl == null)
            return;

        var remainingSafeArea = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);

        if (navBarBorder != null)
        {
            navBarBorder.Padding = navBarBorder.Padding.ApplySafeAreaPadding(new Thickness(SafeAreaPadding.Left, SafeAreaPadding.Top, SafeAreaPadding.Right, 0));
            remainingSafeArea = navBarBorder.Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);
        }

        if (navContentControl.Content is ISafeAreaAware safeAreaAwareChild)
            safeAreaAwareChild.SafeAreaPadding = remainingSafeArea;
        else
            navContentControl.Padding = navContentControl.Padding.ApplySafeAreaPadding(remainingSafeArea);
    }
}
