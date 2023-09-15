namespace RouteNav.Avalonia.Error;

public interface IErrorViewFactory
{
    object BuildErrorView(string message, string? exceptionDetails);
}