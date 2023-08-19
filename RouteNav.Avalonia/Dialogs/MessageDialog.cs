using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace RouteNav.Avalonia.Dialogs;

public partial class MessageDialog : Dialog
{
    #region MessageDialogButtons enum

    public enum MessageDialogButtons
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel
    }

    #endregion

    #region MessageDialogResult enum

    public enum MessageDialogResult
    {
        None,
        Ok,
        Cancel,
        Yes,
        No
    }

    #endregion

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<MessageDialog, string>(nameof(Text));

    public static readonly StyledProperty<MessageDialogButtons> ButtonsProperty = AvaloniaProperty.Register<MessageDialog, MessageDialogButtons>(nameof(Buttons));

    public static readonly StyledProperty<MessageDialogButtonsTemplate> DialogButtonsTemplateProperty = AvaloniaProperty.Register<MessageDialog, MessageDialogButtonsTemplate>(nameof(DialogButtonsTemplate));

    public MessageDialog()
    {
        DialogButtonsTemplate = new MessageDialogButtonsTemplate();
    }

    public string Text
    {
        get { return GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public MessageDialogButtons Buttons
    {
        get { return GetValue(ButtonsProperty); }
        set { SetValue(ButtonsProperty, value); }
    }

    public MessageDialogResult Result { get; internal set; } = MessageDialogResult.None;

    public MessageDialogButtonsTemplate DialogButtonsTemplate
    {
        get { return GetValue(DialogButtonsTemplateProperty); }
        set { SetValue(DialogButtonsTemplateProperty, value); }
    }

    #region StaticHelper

    //public static Task<MessageDialogResult> Show(string title, string text, MessageDialogButtons buttons, Window? parentWindow = null)
    //{
    //    var tcs = new TaskCompletionSource<MessageDialogResult>();

    //    var messageDialog = new MessageDialog { Title = title, Text = text, Buttons = buttons };
    //    messageDialog.Closed += (_, _) => tcs.TrySetResult(messageDialog.Result);

    //    Navigation.UIPlatform.WindowManager.OpenDialog(messageDialog, parentWindow); // TODO: Replaces any open dialog (dangerous!)

    //    return tcs.Task;
    //}

    #region Error

    public static Task Error(string message)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            Navigation.GetMainStack().PushDialogAsync(ErrorPage(message));
        }).GetTask();
    }

    public static Dialog ErrorPage(string message)
    {
        return new MessageDialog() { Title = "Exception", Content = new TextBlock { Text = message, Foreground = Brushes.Red } };
    }

    #endregion

    #endregion
}