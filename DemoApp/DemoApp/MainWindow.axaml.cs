using RouteNav.Avalonia;

namespace DemoApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    internal MainWindow(object? content)
        : base(content)
    {
        InitializeComponent();
    }
}