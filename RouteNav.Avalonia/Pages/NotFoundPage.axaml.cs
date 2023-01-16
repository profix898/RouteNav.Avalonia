using Avalonia.Markup.Xaml;

namespace NSE.RouteNav.Pages;

public partial class NotFoundPage : Page
{
    public NotFoundPage()
    {
        AvaloniaXamlLoader.Load(this);

        DataContext = this;
    }
}