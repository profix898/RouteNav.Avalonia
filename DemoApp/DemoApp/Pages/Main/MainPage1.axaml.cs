using Avalonia.Interactivity;
using Avalonia.Threading;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Dialogs;

namespace DemoApp.Pages.Main
{
    public partial class MainPage1 : Page
    {
        public MainPage1()
        {
            InitializeComponent();
        }

        private async void OpenDlgCommand(object? sender, RoutedEventArgs e)
        {
            await new Dialog() { Title = "Dialog Title", Content = "Content here ..." }.ShowDialog(this);

            await Error.ShowDialog("Error message", this);

            var result = await MessageDialog.ShowDialog("MessageDialog Title", "Do you agree?", MessageDialog.MessageDialogButtons.YesNo);
        }

        private async void OpenDlgEmbeddedCommand(object? sender, RoutedEventArgs e)
        {
            var previousContent = Content;

            await new Dialog() { Title = "Dialog Title", Content = "Content here ..." }.ShowDialogEmbedded(this);

            var result = await new MessageDialog { Title = "MessageDialog Title", Content = "Do you agree?", Buttons = MessageDialog.MessageDialogButtons.YesNo }.ShowDialogEmbedded(this);

            Dispatcher.UIThread.Invoke(() => Content = previousContent);
        }
    }
}
