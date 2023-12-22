namespace RouteNav.Avalonia.Errors;

public interface IErrorViewFactory
{
    object BuildErrorView(string message, string? exceptionDetails);
}