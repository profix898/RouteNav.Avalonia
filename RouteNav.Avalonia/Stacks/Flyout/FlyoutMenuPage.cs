using Avalonia.Controls;

namespace NSE.RouteNav.Stacks.Flyout;

public abstract class FlyoutMenuPage : Page
{
    public virtual ListBox GetListBox()
    {
        return this.FindControl<ListBox>("listBox");
    }
}