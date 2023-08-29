using System;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Platform;

public interface IUIPlatform
{
    public IWindowManager WindowManager { get; }

    #region Pages

    void RegisterPage(params Type[] pageTypes);

    Page GetPage(Type pageType, Uri uri, params object[] parameters);

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

    INavigationStack? ActivateStack(string stackName, INavigationStack? sourceStack = null);

    INavigationStack? ActivateStackInWindow(string stackName);

    #endregion
}