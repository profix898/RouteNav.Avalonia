namespace NSE.RouteNav.Stacks.Flyout;

//public class FlyoutPageStack : NavigationStackBase<FlyoutPage>, INavigationStack
//{
//    private readonly List<FlyoutMenuItem> menuItems = new List<FlyoutMenuItem>();

//    private readonly Type flyoutPageType;

//    public FlyoutPageStack(string name, Type flyoutPageType)
//        : base(name)
//    {
//        if (String.IsNullOrEmpty(name))
//            throw new ArgumentNullException(nameof(name));

//        if (!flyoutPageType.IsSubclassOf(typeof(FlyoutMenuPage)))
//            throw new ArgumentException($"Flyout page does not inherit from '{nameof(FlyoutMenuPage)}'.", nameof(flyoutPageType));

//        this.flyoutPageType = flyoutPageType;
//    }

//    #region Properties

//    public FlyoutLayoutBehavior FlyoutLayoutBehavior { get; set; } = FlyoutLayoutBehavior.Default;

//    #endregion

//    public IReadOnlyList<FlyoutMenuItem> MenuItems => menuItems;

//    public void AddMenuItem(FlyoutMenuItem item)
//    {
//        if (item == null)
//            throw new ArgumentNullException(nameof(item));

//        menuItems.Add(item);
//    }

//    #region Overrides of NavigationStackBase<FlyoutPage>

//    public override Task PushAsync(Page page, bool animated)
//    {
//        Container.Value.Detail = new NavigationPage(page);

//        return Task.CompletedTask;
//    }

//    protected override Page? ResolveRoute(Uri routeUri)
//    {
//        // Resolve via absolute URI
//        if (!routeUri.IsAbsoluteUri)
//            routeUri = this.BuildRoute(routeUri);

//        var flyoutPageItem = menuItems.FirstOrDefault(item => this.GetRoutePath(routeUri)?.Equals(this.GetRoutePath(item.RouteUri)) ?? false);

//        // Try page factory first ...
//        if (flyoutPageItem?.PageFactory != null)
//            return flyoutPageItem.PageFactory();

//        // ... or create instance of page type
//        if (flyoutPageItem?.PageType != null)
//            return Navigation.UIPlatform.GetPage(flyoutPageItem.PageType);

//        return null;
//    }

//    public override Page RootPage => ((NavigationPage) Container.Value.Detail).RootPage;

//    public override Page CurrentPage => ((NavigationPage) Container.Value.Detail).CurrentPage;

//    public override IPageNavigation CurrentPageNavigation => Container.Value.Detail.Navigation;

//    protected override FlyoutPage InitContainer()
//    {
//        // Create container page and select root page
//        var flyoutDetailPage = new FlyoutPage();
//        flyoutDetailPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior;

//        // Flyout page items
//        var flyoutPage = (FlyoutMenuPage?) Navigation.UIPlatform.GetPage(flyoutPageType);
//        if (flyoutPage == null)
//            throw new NavigationException($"{nameof(FlyoutMenuPage)} of type '{flyoutPageType}' can not be resolved.");
//        var listView = flyoutPage.GetListBox();
//        listView.ItemsSource = menuItems;
//        listView.ItemSelected += OnItemSelected;
//        flyoutDetailPage.Flyout = flyoutPage;
//        flyoutDetailPage.Detail = new NavigationPage();

//        return flyoutDetailPage;
//    }

//    private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
//    {
//        if (!(e.SelectedItem is FlyoutMenuItem flyoutPageItem))
//            return;

//        ((ListView) sender).SelectedItem = null;
//        Container.Value.IsPresented = false;

//        Navigation.PushAsync(flyoutPageItem.RouteUri, flyoutPageItem.Target);
//    }

//    public override INavigationStack? RequestStack(string stackName)
//    {
//        if (stackName.Equals(Name, StringComparison.InvariantCulture))
//            return this;
            
//        return null;
//    }

//    public override void AddPage(string relativeRoute, Func<Page> pageFactory)
//    {
//        throw new NotSupportedException($"Use .{nameof(AddMenuItem)}() for {nameof(FlyoutPageStack)} instead.");
//    }

//    #endregion
//}