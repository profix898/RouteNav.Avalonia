using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouteNav.Avalonia.Pages;

public interface IPageNavigation
{
    public event Action<(Page? pageFrom, Page? pageTo)> PageNavigated;

    IReadOnlyList<Page> PageStack { get; }

    Page CurrentPage { get; }

    void InsertPageBefore(Page page, Page beforePage);

    void RemovePage(Page page);

    Task PushAsync(Page page);

    Task<Page> PopAsync();

    Task PopToRootAsync();
}
