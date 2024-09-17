using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouteNav.Avalonia.Pages;

/// <summary>Defines the contract for page navigation (within a navigation container).</summary>
public interface IPageNavigation
{
    /// <summary>Occurs when a page is navigated (from Page -> to Page).</summary>
    public event Action<NavigationEventArgs<Page>> PageNavigated;

    /// <summary>Gets the stack of pages.</summary>
    IReadOnlyList<Page> PageStack { get; }

    /// <summary>Gets the current page.</summary>
    Page? CurrentPage { get; }

    /// <summary>Inserts a page before another page on the stack.</summary>
    /// <param name="page">Page to be inserted in the stack.</param>
    /// <param name="beforePage">Page before which the new page should be inserted.</param>
    void InsertPageBefore(Page page, Page beforePage);

    /// <summary>Removes a page from the stack.</summary>
    /// <param name="page">Page to be removed from the stack.</param>
    void RemovePage(Page page);

    /// <summary>Pushes a page onto the stack.</summary>
    /// <param name="page">Page to be pushed onto the stack.</param>
    /// <returns>Task with the new page as the result.</returns>
    Task<Page> PushAsync(Page page);

    /// <summary>Pops the last page from the stack.</summary>
    /// <returns>Task with the page as the result.</returns>
    Task<Page> PopAsync();

    /// <summary>Pops all pages to the root page.</summary>
    Task PopToRootAsync();
}
