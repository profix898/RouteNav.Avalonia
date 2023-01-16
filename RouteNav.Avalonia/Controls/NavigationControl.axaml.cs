using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace NSE.RouteNav.Controls;

public class NavigationControl : UserControl
{
    private Button? backButton;
    private Button? forwardButton;
    private TransitioningContentControl contentPresenter;
    private INavigationRouter? navigationRouter;

    /// <summary>
    /// Defines the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<NavigationControl, object?>(nameof(Header));

    /// <summary>
    /// Defines the <see cref="PageTransition" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<NavigationControl, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Defines the <see cref="IsNavBarVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool?> IsNavBarVisibleProperty =
        AvaloniaProperty.Register<NavigationControl, bool?>(nameof(IsNavBarVisible), true);

    /// <summary>
    /// Defines the <see cref="CanGoBack" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationControl, bool?> CanGoBackProperty =
        AvaloniaProperty.RegisterDirect<NavigationControl, bool?>(nameof(CanGoBack), o => o.CanGoBack);

    /// <summary>
    /// Defines the <see cref="CanGoForward" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationControl, bool?> CanGoForwardProperty =
        AvaloniaProperty.RegisterDirect<NavigationControl, bool?>(nameof(CanGoForward), o => o.CanGoForward);

    /// <summary>
    /// Defines the <see cref="IsBackButtonVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool?> IsBackButtonVisibleProperty =
        AvaloniaProperty.Register<NavigationControl, bool?>(nameof(IsBackButtonVisible), true);

    /// <summary>
    /// Defines the <see cref="IsForwardButtonVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool?> IsForwardButtonVisibleProperty =
        AvaloniaProperty.Register<NavigationControl, bool?>(nameof(IsForwardButtonVisible), false);

    /// <summary>
    /// Defines the <see cref="INavigationRouter" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationControl, INavigationRouter?> NavigationRouterProperty =
        AvaloniaProperty.RegisterDirect<NavigationControl, INavigationRouter?>(nameof(NavigationRouter), o => o.NavigationRouter, (o, v) => o.NavigationRouter = v);

    /// <summary>
    /// Defines the <see cref="PageTransition" /> property.
    /// </summary>
    public static readonly StyledProperty<IPageTransition?> PageTransitionProperty =
        AvaloniaProperty.Register<NavigationControl, IPageTransition?>(nameof(PageTransition), new CrossFade(TimeSpan.FromSeconds(0.1)));

    public NavigationControl()
    {
        AvaloniaXamlLoader.Load(this);

        backButton = this.FindControl<Button>("NavBackButton");
        if (backButton != null)
            backButton.Click += BackButton_Clicked;

        forwardButton = this.FindControl<Button>("NavForwardButton");
        if (forwardButton != null)
            forwardButton.Click += ForwardButton_Clicked;

        contentPresenter = this.FindControl<TransitioningContentControl>("NavContentPresenter")
                           ?? throw new InvalidOperationException("Can't find ContentPresenter 'NavContentPresenter' in XAML.");

        DataContext = this;
    }

    /// <summary>
    /// Gets or sets the header content
    /// </summary>
    public object? Header
    {
        get { return GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }

    /// <summary>
    /// Gets or sets the header template
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get { return GetValue(HeaderTemplateProperty); }
        set { SetValue(HeaderTemplateProperty, value); }
    }

    /// <summary>
    /// Gets or sets the visibility of the navigation bar
    /// </summary>
    public bool? IsNavBarVisible
    {
        get { return GetValue(IsNavBarVisibleProperty); }
        set { SetValue(IsNavBarVisibleProperty, value); }
    }

    /// <summary>
    /// Gets whether it's possible to go back in the stack
    /// </summary>
    public bool? CanGoBack
    {
        get { return NavigationRouter?.CanGoBack; }
    }

    /// <summary>
    /// Gets whether it's possible to go forward in the stack
    /// </summary>
    public bool? CanGoForward
    {
        get { return NavigationRouter?.CanGoForward; }
    }

    /// <summary>
    /// Gets or sets the visibility of the back button
    /// </summary>
    public bool? IsBackButtonVisible
    {
        get { return GetValue(IsBackButtonVisibleProperty); }
        set { SetValue(IsBackButtonVisibleProperty, value); }
    }

    /// <summary>
    /// Gets or sets the visibility of the forward button
    /// </summary>
    public bool? IsForwardButtonVisible
    {
        get { return GetValue(IsForwardButtonVisibleProperty); }
        set { SetValue(IsForwardButtonVisibleProperty, value); }
    }

    /// <summary>
    /// Gets or sets the navigation router
    /// </summary>
    public INavigationRouter? NavigationRouter
    {
        get { return navigationRouter; }
        set
        {
            var oldRouter = NavigationRouter;

            if (oldRouter != null)
                oldRouter.Navigated -= NavigationRouter_Navigated;

            SetAndRaise(NavigationRouterProperty, ref navigationRouter, value);

            if (value != null)
                value.Navigated += NavigationRouter_Navigated;
        }
    }

    /// <summary>
    /// Gets or sets the animation played when content is changed
    /// </summary>
    public IPageTransition? PageTransition
    {
        get { return GetValue(PageTransitionProperty); }
        set { SetValue(PageTransitionProperty, value); }
    }

    private async void BackButton_Clicked(object? sender, RoutedEventArgs eventArgs)
    {
        if (NavigationRouter != null)
            await NavigationRouter.BackAsync();
    }

    private async void ForwardButton_Clicked(object? sender, RoutedEventArgs eventArgs)
    {
        if (NavigationRouter != null)
            await NavigationRouter.ForwardAsync();
    }

    private void NavigationRouter_Navigated(object? sender, NavigatedEventArgs e)
    {
        contentPresenter.Content = e.To;
        if (e.To is Page page)
            Header = page.Title;

        RaisePropertyChanged(CanGoBackProperty, null, CanGoBack);
        RaisePropertyChanged(CanGoForwardProperty, null, CanGoForward);
    }
}
