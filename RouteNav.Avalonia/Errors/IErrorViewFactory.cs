namespace RouteNav.Avalonia.Errors;

/// <summary>Builds the visual shown for errors (error pages and error dialogs).</summary>
public interface IErrorViewFactory
{
    /// <summary>Builds an error view from a message and optional exception details.</summary>
    object BuildErrorView(string message, string? exceptionDetails);
}
