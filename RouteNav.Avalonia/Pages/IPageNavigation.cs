using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouteNav.Avalonia.Pages;

public interface IPageNavigation
{
    public event Action<NavigationEventArgs<Page>> PageNavigated;

    IReadOnlyList<Page> PageStack { get; }

    Page CurrentPage { get; }

    void InsertPageBefore(Page page, Page beforePage);

    void RemovePage(Page page);

    Task PushAsync(Page page);

    Task<Page> PopAsync();

    Task PopToRootAsync();
}
