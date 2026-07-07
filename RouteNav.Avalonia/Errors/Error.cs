using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using RouteNav.Avalonia.Dialogs;

namespace RouteNav.Avalonia.Errors;

/// <summary>Factory helpers for building and showing error pages and error dialogs.</summary>
public static class Error
{
    /// <summary>Gets or sets the factory used to build the error view content.</summary>
    public static IErrorViewFactory ErrorFactory { get; set; } = new ErrorViewFactory();

    /// <summary>Builds an error page from an exception.</summary>
    public static Page Page(Exception exception)
    {
        return new Page { Title = "Error", Classes = { "Error" }, Content = ErrorFactory.BuildErrorView(exception.Message, ExceptionFormatter.ToString(exception)) };
    }

    /// <summary>Builds an error page from a message and an exception.</summary>
    public static Page Page(string message, Exception exception)
    {
        return new Page { Title = "Error", Classes = { "Error" }, Content = ErrorFactory.BuildErrorView(message, ExceptionFormatter.ToString(exception)) };
    }

    /// <summary>Builds an error page from a message and optional exception details.</summary>
    public static Page Page(string message, string? exceptionDetails = null)
    {
        return new Page { Title = "Error", Classes = { "Error" }, Content = ErrorFactory.BuildErrorView(message, exceptionDetails) };
    }

    /// <summary>Builds an error dialog from an exception.</summary>
    public static Dialog Dialog(Exception exception)
    {
        return new MessageDialog
        {
            Title = "Error", Classes = { "Error" }, Content = ErrorFactory.BuildErrorView(exception.Message, ExceptionFormatter.ToString(exception)),
            Buttons = MessageDialogButtons.Ok
        };
    }

    /// <summary>Builds an error dialog from a message and an exception.</summary>
    public static Dialog Dialog(string message, Exception exception)
    {
        return new MessageDialog
        {
            Title = "Error", Classes = { "Error" }, Content = ErrorFactory.BuildErrorView(message, ExceptionFormatter.ToString(exception)), Buttons = MessageDialogButtons.Ok
        };
    }

    /// <summary>Builds an error dialog from a message and optional exception details.</summary>
    public static Dialog Dialog(string message, string? exceptionDetails = null)
    {
        return new MessageDialog { Title = "Error", Classes = { "Error" }, Content = ErrorFactory.BuildErrorView(message, exceptionDetails), Buttons = MessageDialogButtons.Ok };
    }

    #region ShowDialog

    /// <summary>Shows an error dialog for an exception (optionally in the given parent window).</summary>
    public static Task ShowDialog(Exception exception, Window? parentWindow = null, bool forceOverlay = false)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(exception).ShowDialog(parentWindow, forceOverlay));
    }

    /// <summary>Shows an error dialog for a message and exception (optionally in the given parent window).</summary>
    public static Task ShowDialog(string message, Exception exception, Window? parentWindow = null, bool forceOverlay = false)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(message, exception).ShowDialog(parentWindow, forceOverlay));
    }

    /// <summary>Shows an error dialog for a message and optional details (optionally in the given parent window).</summary>
    public static Task ShowDialog(string message, string? exceptionDetails, Window? parentWindow = null, bool forceOverlay = false)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(message, exceptionDetails).ShowDialog(parentWindow, forceOverlay));
    }

    /// <summary>Shows an error dialog for a message and exception, anchored to the given parent page.</summary>
    public static Task ShowDialog(string message, Exception exception, Page? parentPage, bool forceOverlay = false)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(message, exception).ShowDialog(parentPage, forceOverlay));
    }

    /// <summary>Shows an error dialog for an exception, anchored to the given parent page.</summary>
    public static Task ShowDialog(Exception exception, Page? parentPage, bool forceOverlay = false)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(exception).ShowDialog(parentPage, forceOverlay));
    }

    /// <summary>Shows an error dialog for a message and optional details, anchored to the given parent page.</summary>
    public static Task ShowDialog(string message, string? exceptionDetails, Page? parentPage, bool forceOverlay = false)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(message, exceptionDetails).ShowDialog(parentPage, forceOverlay));
    }

    #endregion
}
