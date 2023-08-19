using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using RouteNav.Avalonia.Internal;

namespace RouteNav.Avalonia.StackControls;

[TemplatePart("PART_NavigationBar", typeof(Border))]
[TemplatePart("PART_NavigationBarBackButton", typeof(Button))]
[TemplatePart("PART_NavigationBarTitle", typeof(ContentPresenter))]
public class NavigationPageContainer : NavigationContainer
{
    private Border? navBarBorder;
    private Button? navBarBackButton;
    private ContentPresenter? navBarTitle;
    private TransitioningContentControl? contentControl;

    public static readonly StyledProperty<IBrush?> NavBarBackgroundProperty = AvaloniaProperty.Register<NavigationPageContainer, IBrush?>(nameof(NavBarBackground));

    public static readonly StyledProperty<IBrush?> NavBarTextColorProperty = AvaloniaProperty.Register<NavigationPageContainer, IBrush?>(nameof(NavBarTextColor));

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
            NavigationStack?.PopAsync();
    }

    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        navBarBorder = e.NameScope.Get<Border>("PART_NavigationBar");
        if (navBarBackButton != null)
            navBarBackButton.Click -= NavBarBackButton_Clicked;
        navBarBackButton = e.NameScope.Get<Button>("PART_NavigationBarBackButton");
        if (navBarBackButton != null)
            navBarBackButton.Click += NavBarBackButton_Clicked;
        navBarTitle = e.NameScope.Get<ContentPresenter>("PART_NavigationBarTitle");

        contentControl = e.NameScope.Get<TransitioningContentControl>("PART_ContentPresenter");
        contentControl.PageTransition = PageTransition;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PageTransitionProperty && contentControl != null)
            contentControl.PageTransition = PageTransition;
    }

    protected override void UpdateContentSafeAreaPadding()
    {
        if (Content != null && Presenter != null)
        {
            var remainingSafeArea = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);

            if (navBarBorder != null)
            {
                navBarBorder.Padding = new Thickness(SafeAreaPadding.Left, SafeAreaPadding.Top, SafeAreaPadding.Right, 0);
                remainingSafeArea = navBarBorder.Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);
            }

            if (Presenter.Child is ISafeAreaAware safeAreaAware)
                safeAreaAware.SafeAreaPadding = remainingSafeArea;
            else
                Presenter.Padding = Presenter.Padding.ApplySafeAreaPadding(remainingSafeArea);
        }
    }

    protected override void OnPageNavigated((Page? pageFrom, Page? pageTo) e)
    {
        RaisePropertyChanged(CanGoBackProperty, null, CanGoBack);
        navBarTitle?.SetValue(ContentPresenter.ContentProperty, e.pageTo?.Title ?? String.Empty);
        Presenter?.SetValue(ContentProperty, e.pageTo);
    }
}
