using Avalonia;
using Avalonia.Controls;

namespace RouteNav.Avalonia.Error;

public partial class ErrorView : UserControl
{
    public static readonly StyledProperty<string> ErrorMessageProperty = AvaloniaProperty.Register<ErrorView, string>(nameof(ErrorMessage));

    public static readonly StyledProperty<string?> ExceptionDetailsProperty = AvaloniaProperty.Register<ErrorView, string?>(nameof(ExceptionDetails));

    public ErrorView()
    {
        InitializeComponent();

        DataContext = this;
    }

    public string ErrorMessage
    {
        get { return GetValue(ErrorMessageProperty); }
        set { SetValue(ErrorMessageProperty, value); }
    }

    public string? ExceptionDetails
    {
        get { return GetValue(ExceptionDetailsProperty); }
        set { SetValue(ExceptionDetailsProperty, value); }
    }
}
