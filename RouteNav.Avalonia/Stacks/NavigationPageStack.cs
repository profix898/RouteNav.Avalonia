using System;
using Avalonia.Layout;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

public class NavigationPageStack : NavigationPageStack<NavigationPageContainer>
{
    public NavigationPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }
}

public class NavigationPageStack<TC> : NavigationStackBase<TC>, INavigationStack
    where TC : NavigationPageContainer, new()
{
    public NavigationPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }

    #region Overrides of NavigationStackBase<NavigationPage>

    protected override TC InitContainer()
    {
        RootPage = new LazyValue<Page>(() => ResolveRoute(this.BuildRoute(String.Empty))
                                             ?? throw new NavigationException("RootPage can not be retrieved."));

        var navigationPageContainer = new TC
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            NavigationStack = this
        };
        navigationPageContainer.HostControlAttached += () =>
        {
            if (navigationPageContainer.NavigationControl == null)
                throw new InvalidOperationException($"No {nameof(NavigationControl)} found in NavigationContainer.");
        };

        return navigationPageContainer;
    }
    
    #endregion
}