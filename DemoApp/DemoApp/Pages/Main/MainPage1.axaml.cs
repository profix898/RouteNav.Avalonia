using System;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
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

    private async void OpenDlgSizingCommand(object? sender, RoutedEventArgs e)
    {
        // Fixed width, content-hugging height (the dialog window sizes to its content)
        await new TestDialog { DialogSize = DialogSize.Custom, Width = 520, Title = "Custom width — height hugs content" }.ShowDialog(this);

        // Scale-derived height (Medium) with a fixed width
        await new TestDialog { DialogSize = DialogSize.Medium, Width = 380, Title = "Medium height — fixed width" }.ShowDialog(this);

        // Wrap mode: the frame grows with the content (wrapped text never clips)
        await new Dialog
        {
            Title = "Wrap mode — the frame fits the content",
            DialogSize = DialogSize.Custom,
            Width = 460,
            DialogContentMode = DialogContentMode.Wrap,
            Content = new Avalonia.Controls.TextBlock
            {
                Margin = new Thickness(18, 16),
                MaxWidth = 420,
                TextWrapping = TextWrapping.Wrap,
                Text = "This dialog hosts its content without height constraints: no matter how much text is wrapped " +
                       "into the frame, the dialog keeps growing with it instead of clipping the content behind a fixed size."
            }
        }.ShowDialog(this);
    }

    private async void OpenDlgSizingOverlayCommand(object? sender, RoutedEventArgs e)
    {
        // Overlay dialogs honor custom sizes: the fixed width is kept, the height hugs the content
        await new TestDialog { DialogSize = DialogSize.Custom, Width = 520, Title = "Custom width — hugging height (overlay)" }.ShowDialog(this, true);
    }

    private async void OpenDlgSizingEmbeddedCommand(object? sender, RoutedEventArgs e)
    {
        var previousContent = Content;

        await new TestDialog { DialogSize = DialogSize.Custom, Width = 460, Title = "Custom width — hugging height (embedded)" }.ShowDialogEmbedded(this);

        Dispatcher.UIThread.Invoke(() => Content = previousContent);
    }

    private async void OpenDlgPageHintsCommand(object? sender, RoutedEventArgs e)
    {
        await new TestPage
        {
            DialogSizeHint = DialogSize.Medium,
            SizeScaleHint = new Size(0.4, 0.4),
            MinSizeHint = new Size(300, 260),
            MaxSizeHint = new Size(500, 450),
            Title = "Page with size hints"
        }.ToDialog(this).ShowDialog(this);
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
