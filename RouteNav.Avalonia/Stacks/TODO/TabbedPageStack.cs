namespace RouteNav.Avalonia.Stacks;

//public class TabbedPageStack : NavigationStackBase<TabbedPage>, IPageNavigation, IRouteNavigation, INavigationStack
//{
//    private Page? rootPage;

//    public TabbedPageStack(string name)
//        : base(name)
//    {
//        if (String.IsNullOrEmpty(name))
//            throw new ArgumentNullException(nameof(name));
//    }

//    private Dictionary<string, Func<Page>> Pages { get; } = new Dictionary<string, Func<Page>>();

//    #region Properties

//    public Color BarBackground { get; set; } = Brushes.SteelBlue;

//    public Color BarTextColor { get; set; } = Brushes.Black;

//    public Color SelectedTabColor { get; set; } = Brushes.WhiteSmoke;

//    public Color UnselectedTabColor { get; set; } = Brushes.Grey;

//    #endregion

//    #region Overrides of NavigationStackBase<TabbedPage>

//    protected override Page? ResolveRoute(Uri routeUri)
//    {
//        return Pages.TryGetValue(this.GetRoutePath(routeUri)!, out var pageFactory) ? pageFactory() : null;
//    }

//    public override Page RootPage => rootPage ?? new NotFoundPage();

//    public override Page CurrentPage => Container.Value.CurrentPage;

//    public override IPageNavigation CurrentPageNavigation => Container.Value.Navigation;

//    protected override TabbedPage InitContainer()
//    {
//        // Create container page and select root page
//        var tabbedPage = new TabbedPage();
//        if (!BarBackground.IsDefault())
//            tabbedPage.BarBackground = BarBackground;
//        if (!BarTextColor.IsDefault())
//            tabbedPage.BarTextColor = BarTextColor;
//        if (!SelectedTabColor.IsDefault())
//            tabbedPage.SelectedTabColor = SelectedTabColor;
//        if (!UnselectedTabColor.IsDefault())
//            tabbedPage.UnselectedTabColor = UnselectedTabColor;

//        foreach (var pageKvp in Pages)
//        {
//            var page = pageKvp.Value();
//            tabbedPage.Children.Add(page);

//            if (pageKvp.Key == String.Empty) // Initial page
//                rootPage = tabbedPage.CurrentPage = page;
//        }
//        rootPage ??= tabbedPage.CurrentPage = new NotFoundPage();

//        return tabbedPage;
//    }

//    public override void AddPage(string relativeRoute, Func<Page> pageFactory)
//    {
//        Pages.Set(relativeRoute.TrimStart('/'), pageFactory);
//    }

//    #endregion
//}