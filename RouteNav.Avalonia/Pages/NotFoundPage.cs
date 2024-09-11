using System;
using RouteNav.Avalonia.Dialogs;

namespace RouteNav.Avalonia.Pages;

public class NotFoundPage : Page
{
    public NotFoundPage()
    {
        Title = "Page Not Found";
        DialogSizeHint = DialogSize.Small;
    }

    protected override Type StyleKeyOverride => typeof(NotFoundPage);
}