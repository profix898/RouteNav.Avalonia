using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using RouteNav.Avalonia.Dialogs;

namespace RouteNav.Avalonia;

public static class Error
{
    public static Task ShowDialog(string message, Window? parentWindow = null)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(message).ShowDialog(parentWindow));
    }

    public static Task ShowDialog(string message, Page? parentPage)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(message).ShowDialog(parentPage));
    }

    public static Page Page(string message)
    {
        return new Page
        {
            Title = "Error",
            Classes = { "Error" },
            Content = new TextBlock
            {
                Text = message, Foreground = Brushes.Red,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
    }

    public static Dialog Dialog(string message)
    {
        return new MessageDialog
        {
            Title = "Error",
            Classes = { "Error" },
            Content = new TextBlock
            {
                Text = message, Foreground = Brushes.Red,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
    }
}
