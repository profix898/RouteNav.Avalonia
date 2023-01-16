using System;
using System.Threading.Tasks;

namespace NSE.RouteNav.Stacks;

public class ContentPageStack : NavigationPageStack, INavigationStack
{
    // Note: Due to 'PushAsync is not supported globally on Android, please use a NavigationPage', we can't navigate
    // between Pages directly. Here, Pages are wrapped in a NavigationPage with hidden navigation bar.

    public ContentPageStack(string name)
        : base(name)
    {
        if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
    }

    #region Overrides of NavigationStackBase<TabbedPage>

    protected override Page? ResolveRoute(Uri routeUri)
    {
        var page = base.ResolveRoute(routeUri);
        if (page != null)
        {
            //Container.Value.IsNavBarVisible = false;
            //Container.Value.IsBackButtonVisible = !IsMainStack;
        }

        return page;
    }

    public override Task PushAsync(Page page)
    {
        if (Equals(page, CurrentPage))
            return Task.CompletedTask;

        var previousPage = CurrentPage;

        pageStack[pageStack.IndexOf(previousPage)] = page;
        CurrentPage = page;

        OnPageNavigated(previousPage, CurrentPage);

        return Task.CompletedTask;
    }

    #endregion
}