using Avalonia.Interactivity;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Dialogs;

namespace DemoApp.Pages
{
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }

        private async void OpenDlgCommand(object? sender, RoutedEventArgs e)
        {
            await new Dialog() { Title = "Dialog Title", Content = "Content here ..." }.ShowDialog();

            await MessageDialog.Error("Error message");

            var result = await MessageDialog.ShowDialog("MessageDialog Title", "Do you agree?", MessageDialog.MessageDialogButtons.YesNo);
        }
    }
}
