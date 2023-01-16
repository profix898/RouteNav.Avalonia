using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using NSE.RouteNav.Bootstrap;
using NSE.RouteNav.Platform;

namespace NSE.RouteNav.Dialogs;

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

    public MessageDialog()
    {
        AvaloniaXamlLoader.Load(this);
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

    private MessageDialogButtonsTemplate DialogButtonsTemplate => new MessageDialogButtonsTemplate();

    #region StaticHelper

    public static Task<MessageDialogResult> Show(string title, string text, MessageDialogButtons buttons, Window? parentWindow = null)
    {
        var tcs = new TaskCompletionSource<MessageDialogResult>();

        var messageDialog = new MessageDialog { Title = title, Text = text, Buttons = buttons };
        messageDialog.Closed += (_, _) => tcs.TrySetResult(messageDialog.Result);

        var uiPlatform = AvaloniaLocator.Current.GetService<IUIPlatform>()
                         ?? throw new NavigationException($"Implementation of {nameof(IUIPlatform)} is not available. Bootstrap via {nameof(AppBuilderExtensions.UseRouteNavPlatform)}().");
        uiPlatform.OpenDialog(messageDialog, parentWindow); // TODO: Replaces any open dialog (dangerous!)

        return tcs.Task;
    }

    #region Error

    public static Task Error(string message)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            Navigation.GetMainStack().PushDialogAsync(ErrorPage(message));
        });
    }

    public static Dialog ErrorPage(string message)
    {
        return new MessageDialog() { Title = "Exception", Content = new TextBlock { Text = message, Foreground = Brushes.Red } };
    }

    #endregion

    #endregion
}