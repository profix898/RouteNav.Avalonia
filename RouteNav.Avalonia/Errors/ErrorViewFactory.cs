namespace RouteNav.Avalonia.Errors;

/// <summary>Default <see cref="IErrorViewFactory"/> implementation that builds an <see cref="ErrorView"/>.</summary>
public class ErrorViewFactory : IErrorViewFactory
{
    #region Implementation of IErrorViewFactory

    /// <inheritdoc />
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
