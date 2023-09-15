using System;
using RouteNav.Avalonia.Dialogs;

namespace RouteNav.Avalonia.Pages;

public class NotFoundPage : Page
{
    public NotFoundPage()
    {
        DialogSizeHint = DialogSize.Small;
    }

    protected override Type StyleKeyOverride => typeof(NotFoundPage);
}