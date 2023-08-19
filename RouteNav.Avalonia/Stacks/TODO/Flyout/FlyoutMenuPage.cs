using Avalonia.Controls;

namespace RouteNav.Avalonia.Stacks.TODO.Flyout;

public abstract class FlyoutMenuPage : Page
{
    public virtual ListBox GetListBox()
    {
        return this.FindControl<ListBox>("listBox");
    }
}