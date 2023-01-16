using System;
using System.Threading.Tasks;
using NSE.RouteNav.Stacks;

namespace NSE.RouteNav.Platform;

public interface IUIPlatform
{
    #region Pages

    void RegisterPages(params Type[] pageTypes);

    Page GetPage(Type pageType);

    #endregion

    #region Stacks

    void AddStack(INavigationStack stack);

    void RemoveStack(string stackName);

    INavigationStack? GetStack(string stackName);

    #endregion

    #region ActiveStacks

    INavigationStack GetMainStack();

    INavigationStack? GetActiveStack(string stackName);

    INavigationStack? GetActiveStackFromWindow(Window? window);

    Window? GetActiveWindowFromStack(INavigationStack? navigationStack);

    #endregion

    #region Targets

    INavigationStack? PushToView(string stackName, INavigationStack? sourceStack = null);

    INavigationStack? PushToDialog(string stackName, out Window? parentWindow, INavigationStack? sourceStack = null);

    INavigationStack? PushToWindow(string stackName);

    #endregion

    #region PlatformOpen

    Task OpenDialog(Dialog dialog, Window? parentWindow = null);

    bool OpenWindow(Window window, Window? parentWindow = null);

    #endregion
}