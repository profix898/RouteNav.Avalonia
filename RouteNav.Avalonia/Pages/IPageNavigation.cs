using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSE.RouteNav.Pages;

public interface IPageNavigation
{
    IReadOnlyList<Page> PageStack { get; }

    public event Action<(Page? pageFrom, Page? pageTo)> PageNavigated;

    void InsertPageBefore(Page page, Page beforePage);

    void RemovePage(Page page);

    Task PushAsync(Page page);

    Task<Page> PopAsync();

    Task PopToRootAsync();
}
