using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using RouteNav.Avalonia.Dialogs;
using static RouteNav.Avalonia.Dialogs.MessageDialog;

namespace RouteNav.Avalonia.Error;

public static class Error
{
    public static IErrorViewFactory ErrorFactory { get; set; } = new ErrorViewFactory();

    public static Page Page(Exception exception)
    {
        return new Page
        {
            Title = "Error",
            Classes = { "Error" },
            Content = ErrorFactory.BuildErrorView(exception.Message, ExceptionFormatter.ToString(exception))
        };
    }

    public static Page Page(string message, string? exceptionDetails = null)
    {
        return new Page
        {
            Title = "Error",
            Classes = { "Error" },
            Content = ErrorFactory.BuildErrorView(message, exceptionDetails)
        };
    }

    public static Dialog Dialog(Exception exception)
    {
        return new MessageDialog
        {
            Title = "Error",
            Classes = { "Error" },
            Content = ErrorFactory.BuildErrorView(exception.Message, ExceptionFormatter.ToString(exception)),
            Buttons = MessageDialogButtons.Ok
        };
    }

    public static Dialog Dialog(string message, string? exceptionDetails = null)
    {
        return new MessageDialog
        {
            Title = "Error",
            Classes = { "Error" },
            Content = ErrorFactory.BuildErrorView(message, exceptionDetails),
            Buttons = MessageDialogButtons.Ok
        };
    }

    #region ShowDialog

    public static Task ShowDialog(Exception exception, Window? parentWindow = null, bool forceOverlay = false)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(exception).ShowDialog(parentWindow, forceOverlay));
    }

    public static Task ShowDialog(string message, string? exceptionDetails, Window? parentWindow = null, bool forceOverlay = false)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(message, exceptionDetails).ShowDialog(parentWindow, forceOverlay));
    }

    public static Task ShowDialog(Exception exception, Page? parentPage, bool forceOverlay = false)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(exception).ShowDialog(parentPage, forceOverlay));
    }

    public static Task ShowDialog(string message, string? exceptionDetails, Page? parentPage, bool forceOverlay = false)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Dialog(message, exceptionDetails).ShowDialog(parentPage, forceOverlay));
    }

    #endregion
}
