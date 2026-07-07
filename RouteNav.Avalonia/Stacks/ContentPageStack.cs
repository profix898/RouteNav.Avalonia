using System;
using System.Threading.Tasks;
using Avalonia.Layout;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

/// <summary>A minimal navigation stack that shows a single page at a time (no navigation bar or history).</summary>
public class ContentPageStack : NavigationStackBase<NavigationContainer>, INavigationStack
{
    /// <summary>Initializes a new instance of the <see cref="ContentPageStack"/> class.</summary>
    public ContentPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }

    #region Overrides of NavigationStackBase<NavigationPage>

    /// <inheritdoc />
    protected override NavigationContainer InitContainer()
    {
        RootPage = new LazyValue<Page>(() => ResolveRoute(this.BuildRoute(String.Empty))
                                             ?? throw new NavigationException("RootPage can not be retrieved."));
        
        return new NavigationContainer
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            NavigationStack = this
        };
    }

    /// <inheritdoc />
    public override Task<Page> PushAsync(Page page)
    {
        if (page.Equals(CurrentPage))
            return Task.FromResult(CurrentPage);

        var previousPage = CurrentPage;

        pageStack.Clear();
        pageStack.Add(page);
        CurrentPage = page;

        ContainerPage.Value.UpdatePage(CurrentPage);
        OnPageNavigated(previousPage, CurrentPage);

        return Task.FromResult(CurrentPage);
    }

    #endregion
}