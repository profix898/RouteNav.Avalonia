using System;
using Avalonia.Interactivity;
using Avalonia.Threading;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Error;

namespace DemoApp.Pages.Main;

public partial class MainPage1 : Page
{
    public MainPage1()
    {
        InitializeComponent();
    }

    private async void OpenDlgCommand(object? sender, RoutedEventArgs e)
    {
        await new Dialog { Title = "Dialog Title", Content = new TestPage() }.ShowDialog(this);
        await Error.ShowDialog("Error message", new NotImplementedException("Something went terribly wrong!").ToString(), this);
        var result = await MessageDialog.ShowDialog("MessageDialog Title", "Avalonia UI is fun! Don't you think?", MessageDialog.MessageDialogButtons.YesNo);
    }

    private async void OpenDlgEmbeddedCommand(object? sender, RoutedEventArgs e)
    {
        var previousContent = Content;

        await new Dialog { Title = "Dialog Title", Content = new TestPage() }.ShowDialogEmbedded(this);
        var result = await MessageDialog.ShowDialogEmbedded("MessageDialog Title", "Avalonia UI is fun! Don't you think?", MessageDialog.MessageDialogButtons.YesNo, this);

        Dispatcher.UIThread.Invoke(() => Content = previousContent);
    }

    private async void OpenDlgPageCommand(object? sender, RoutedEventArgs e)
    {
        //await new TestPage().ToDialog(this).ShowDialog(this);
        await new TestPage { DialogSizeHint = DialogSize.Small }.ToDialog(this).ShowDialog(this);
        await new TestPage { DialogSizeHint = DialogSize.Medium }.ToDialog(this).ShowDialog(this);
        //await new TestPage { DialogSizeHint = DialogSize.Small }.ToDialog(this, DialogSize.Medium).ShowDialog(this);
        await new TestPage { DialogSizeHint = DialogSize.Custom, Width = 456, Height = 234 }.ToDialog(this).ShowDialog(this);
    }

    private async void OpenDlgPageOverlayCommand(object? sender, RoutedEventArgs e)
    {
        //await new TestPage().ToDialog(this).ShowDialog(this, true);
        await new TestPage { DialogSizeHint = DialogSize.Small }.ToDialog(this).ShowDialog(this, true);
        await new TestPage { DialogSizeHint = DialogSize.Medium }.ToDialog(this).ShowDialog(this, true);
        //await new TestPage { DialogSizeHint = DialogSize.Small }.ToDialog(this, DialogSize.Medium).ShowDialog(this, true);
        await new TestPage { DialogSizeHint = DialogSize.Custom, Width = 456, Height = 234 }.ToDialog(this).ShowDialog(this, true);
    }
}
