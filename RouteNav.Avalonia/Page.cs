using Avalonia;
using Avalonia.Controls;

namespace NSE.RouteNav;

public class Page : UserControl
{
    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Page, string>(nameof(Title), "Page");

    /// <summary>
    /// Gets or sets the title of the page.
    /// </summary>
    public string Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }
}