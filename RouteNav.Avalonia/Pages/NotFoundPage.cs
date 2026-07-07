using System;
using RouteNav.Avalonia.Dialogs;

namespace RouteNav.Avalonia.Pages;

/// <summary>Default page shown when a route cannot be resolved.</summary>
public class NotFoundPage : Page
{
    /// <summary>Initializes a new instance of the <see cref="NotFoundPage" /> class.</summary>
    public NotFoundPage()
    {
        Title = "Page Not Found";
        DialogSizeHint = DialogSize.Small;
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(NotFoundPage);
}
