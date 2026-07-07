using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Internal;

namespace RouteNav.Avalonia.StackContainers;

/// <summary>A navigation container that hosts pages in a <see cref="NavigationControl"/>.</summary>
public class NavigationPageContainer : NavigationContainer
{
    /// <summary>Defines the <see cref="HideNavigationBarForRootPage"/> property.</summary>
    public static readonly StyledProperty<bool> HideNavigationBarForRootPageProperty = AvaloniaProperty.Register<NavigationPageContainer, bool>(nameof(HideNavigationBarForRootPage));

    /// <summary>Defines the <see cref="NavigationControlName"/> property.</summary>
    public static readonly StyledProperty<string> NavigationControlNameProperty = AvaloniaProperty.Register<NavigationPageContainer, string>(nameof(NavigationControlName), "NavigationControl");

    /// <summary>Initializes a new instance of the <see cref="NavigationPageContainer"/> class.</summary>
    public NavigationPageContainer()
    {
        RegisterScopedControl(this, NavigationControlName, Content = new NavigationControl());
    }

    /// <summary>Gets or sets a value indicating whether the navigation bar is hidden for the root page.</summary>
    public bool HideNavigationBarForRootPage
    {
        get { return GetValue(HideNavigationBarForRootPageProperty); }
        set { SetValue(HideNavigationBarForRootPageProperty, value); }
    }

    /// <summary>Gets or sets the scoped name used to resolve the navigation control.</summary>
    public string NavigationControlName
    {
        get { return GetValue(NavigationControlNameProperty); }
        set { SetValue(NavigationControlNameProperty, value); }
    }

    /// <summary>Gets the resolved navigation control.</summary>
    public NavigationControl? NavigationControl { get; private set; }

    /// <inheritdoc />
    public override void UpdatePage(Page page)
    {
        if (NavigationControl != null)
        {
            NavigationControl.Page = page;
            NavigationControl.BackButtonEnabled = NavigationStack != null && (NavigationStack.PageStack.Count > (NavigationStack.IsMainStack ? 1 : 0));

            if (HideNavigationBarForRootPage)
                NavigationControl.NavigationBarVisible = !page.Equals(NavigationStack.RootPage.Value);
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (NavigationControl != null)
            NavigationControl.BackButtonClick -= BackButton_Clicked;
        NavigationControl = this.GetControl<NavigationControl>(NavigationControlName);
        if (NavigationControl != null)
            NavigationControl.BackButtonClick += BackButton_Clicked;

        OnHostControlAttached();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == NavigationControlNameProperty)
        {
            if (NavigationControl != null)
                NavigationControl.BackButtonClick -= BackButton_Clicked;
            NavigationControl = this.GetControl<NavigationControl>(NavigationControlName);
            if (NavigationControl != null)
                NavigationControl.BackButtonClick += BackButton_Clicked;

            OnHostControlAttached();
        }
    }

    /// <inheritdoc />
    protected override void UpdateContentSafeAreaPadding()
    {
        if (NavigationControl != null)
            NavigationControl.SafeAreaPadding = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);
    }

    private void BackButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (NavigationControl != null && NavigationControl.BackButtonEnabled)
            Navigation.PopAsync(NavigationStack);
    }
}
