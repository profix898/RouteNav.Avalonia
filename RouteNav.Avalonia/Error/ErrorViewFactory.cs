namespace RouteNav.Avalonia.Error;

public class ErrorViewFactory : IErrorViewFactory
{
    #region Implementation of IErrorViewFactory

    public object BuildErrorView(string message, string? exceptionDetails)
    {
        return new ErrorView
        {
            ErrorMessage = message,
            ExceptionDetails = exceptionDetails
        };
    }

    #endregion
}
