using RouteNav.Avalonia.Dialogs;
using System;

namespace RouteNav.Avalonia.Pages;

public class InternalErrorPage : Page
{
    public InternalErrorPage()
    {
        DialogSizeHint = DialogSize.Small;
    }

    protected override Type StyleKeyOverride => typeof(InternalErrorPage);
}