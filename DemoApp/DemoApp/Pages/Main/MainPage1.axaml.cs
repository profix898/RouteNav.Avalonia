using System;
using Avalonia.Interactivity;
using Avalonia.Threading;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Errors;

namespace DemoApp.Pages.Main;

public partial class MainPage1 : Page
{
    public MainPage1()
    {
        InitializeComponent();
    }
    
    private async void OpenMsgDlgCommand(object? sender, RoutedEventArgs e)
    {
        var result = await MessageDialog.Create("MessageDialog Title", "Avalonia is fun! Don't you think?", MessageDialogButtons.YesNo).ShowDialog(this);
        
        await Error.ShowDialog("Error message", new NotImplementedException("Something went terribly wrong!"), this);
    }

    private async void OpenDlgCommand(object? sender, RoutedEventArgs e)
    {
        await new TestDialog { DialogSize = DialogSize.Small, Title = "Small dialog" }.ShowDialog(this);
        await new TestDialog { DialogSize = DialogSize.Medium, Title = "Medium dialog" }.ShowDialog(this);
        await new TestDialog { DialogSize = DialogSize.Large, Title = "Large dialog" }.ShowDialog(this);
        await new TestDialog { DialogSize = DialogSize.Custom, Width = 456, Height = 234, Title = "Custom-sized dialog" }.ShowDialog(this);
    }

    private async void OpenDlgEmbeddedCommand(object? sender, RoutedEventArgs e)
    {
        var previousContent = Content;
        
        await new TestDialog { DialogSize = DialogSize.Small, Title = "Small dialog" }.ShowDialogEmbedded(this);
        await new TestDialog { DialogSize = DialogSize.Medium, Title = "Medium dialog" }.ShowDialogEmbedded(this);

        await MessageDialog.Create("MessageDialog Title", "Avalonia is fun! Don't you think?", MessageDialogButtons.YesNo).ShowDialogEmbedded(this);

        Dispatcher.UIThread.Invoke(() => Content = previousContent);
    }

    private async void OpenDlgPageCommand(object? sender, RoutedEventArgs e)
    {
        await new TestPage { DialogSizeHint = DialogSize.Small, Title = "Small page" }.ToDialog(this).ShowDialog(this);
        await new TestPage { DialogSizeHint = DialogSize.Medium, Title = "Medium page" }.ToDialog(this).ShowDialog(this);
        await new TestPage { DialogSizeHint = DialogSize.Large, Title = "Large page" }.ToDialog(this).ShowDialog(this);
        await new TestPage { DialogSizeHint = DialogSize.Custom, Width = 456, Height = 234, Title = "Custom-sized page" }.ToDialog(this).ShowDialog(this);
    }

    private async void OpenDlgPageOverlayCommand(object? sender, RoutedEventArgs e)
    {
        await new TestPage { DialogSizeHint = DialogSize.Small, Title = "Small page" }.ToDialog(this).ShowDialog(this, true);
        await new TestPage { DialogSizeHint = DialogSize.Medium, Title = "Medium page" }.ToDialog(this).ShowDialog(this, true);
        await new TestPage { DialogSizeHint = DialogSize.Large, Title = "Large page" }.ToDialog(this).ShowDialog(this, true);
        await new TestPage { DialogSizeHint = DialogSize.Custom, Width = 456, Height = 234, Title = "Custom-sized page" }.ToDialog(this).ShowDialog(this, true);
    }
}
