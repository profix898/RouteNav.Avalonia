using System;
using Avalonia.Layout;
using RouteNav.Avalonia.Controls;
using RouteNav.Avalonia.StackContainers;

namespace RouteNav.Avalonia.Stacks;

/// <summary>A navigation stack that presents pages with a navigation bar and back-button history.</summary>
public class NavigationPageStack : NavigationPageStack<NavigationPageContainer>
{
    /// <summary>Initializes a new instance of the <see cref="NavigationPageStack" /> class.</summary>
    public NavigationPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }
}

/// <summary>A navigation stack that presents pages with a navigation bar and back-button history.</summary>
/// <typeparam name="TC">The navigation container type.</typeparam>
public class NavigationPageStack<TC> : NavigationStackBase<TC>, INavigationStack
    where TC : NavigationPageContainer, new()
{
    /// <summary>Initializes a new instance of the <see cref="NavigationPageStack{TC}" /> class.</summary>
    public NavigationPageStack(string name, string title)
        : base(name, title)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (String.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));
    }

    #region Overrides of NavigationStackBase<NavigationPage>

    /// <inheritdoc />
    protected override TC InitContainer()
    {
        RootPage = new LazyValue<Page>(() => ResolveRoute(this.BuildRoute(String.Empty))
                                             ?? throw new NavigationException("RootPage can not be retrieved."));

        var navigationPageContainer = new TC { VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, NavigationStack = this };
        navigationPageContainer.HostControlAttached += () =>
        {
            if (navigationPageContainer.NavigationControl == null)
                throw new InvalidOperationException($"No {nameof(NavigationControl)} found in NavigationContainer.");
        };

        return navigationPageContainer;
    }

    #endregion
}
