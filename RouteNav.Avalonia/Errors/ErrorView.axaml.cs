using Avalonia;
using Avalonia.Controls;

namespace RouteNav.Avalonia.Errors;

/// <summary>Default view used to display error messages and optional exception details.</summary>
public partial class ErrorView : UserControl
{
    /// <summary>Defines the <see cref="ErrorMessage" /> property.</summary>
    public static readonly StyledProperty<string> ErrorMessageProperty = AvaloniaProperty.Register<ErrorView, string>(nameof(ErrorMessage));

    /// <summary>Defines the <see cref="ExceptionDetails" /> property.</summary>
    public static readonly StyledProperty<string?> ExceptionDetailsProperty = AvaloniaProperty.Register<ErrorView, string?>(nameof(ExceptionDetails));

    /// <summary>Initializes a new instance of the <see cref="ErrorView" /> class.</summary>
    public ErrorView()
    {
        InitializeComponent();

        DataContext = this;
    }

    /// <summary>Gets or sets the user-facing error message.</summary>
    public string ErrorMessage
    {
        get { return GetValue(ErrorMessageProperty); }
        set { SetValue(ErrorMessageProperty, value); }
    }

    /// <summary>Gets or sets optional formatted exception details.</summary>
    public string? ExceptionDetails
    {
        get { return GetValue(ExceptionDetailsProperty); }
        set { SetValue(ExceptionDetailsProperty, value); }
    }
}
