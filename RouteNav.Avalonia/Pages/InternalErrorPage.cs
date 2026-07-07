using RouteNav.Avalonia.Dialogs;
using System;

namespace RouteNav.Avalonia.Pages;

/// <summary>Default page shown when a page or route fails due to an internal exception.</summary>
public class InternalErrorPage : Page
{
    /// <summary>Initializes a new instance of the <see cref="InternalErrorPage"/> class.</summary>
    public InternalErrorPage()
    {
        Title = "Internal Error";
        DialogSizeHint = DialogSize.Small;
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(InternalErrorPage);
}