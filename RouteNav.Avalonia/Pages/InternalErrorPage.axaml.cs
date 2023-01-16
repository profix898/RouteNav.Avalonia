using Avalonia.Markup.Xaml;

namespace NSE.RouteNav.Pages;

public partial class InternalErrorPage : Page
{
    public InternalErrorPage()
    {
        AvaloniaXamlLoader.Load(this);

        DataContext = this;
    }
}