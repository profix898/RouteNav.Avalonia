using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.Internal;

namespace RouteNav.Avalonia.StackContainers;

public class NavigationPageContainer : NavigationContainer
{
    public static readonly StyledProperty<bool> HideNavigationBarForRootPageProperty = AvaloniaProperty.Register<NavigationPageContainer, bool>(nameof(HideNavigationBarForRootPage));

    public static readonly StyledProperty<string> NavigationControlNameProperty = AvaloniaProperty.Register<NavigationPageContainer, string>(nameof(NavigationControlName), "NavigationControl");

    public NavigationPageContainer()
    {
        RegisterScopedControl(this, NavigationControlName, Content = new NavigationControl());
    }

    public bool HideNavigationBarForRootPage
    {
        get { return GetValue(HideNavigationBarForRootPageProperty); }
        set { SetValue(HideNavigationBarForRootPageProperty, value); }
    }

    public string NavigationControlName
    {
        get { return GetValue(NavigationControlNameProperty); }
        set { SetValue(NavigationControlNameProperty, value); }
    }

    public NavigationControl? NavigationControl { get; private set; }

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
