using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace RouteNav.Avalonia.Dialogs;

[TemplatePart("PART_DialogButtons", typeof(ContentPresenter))]
public class MessageDialog : Dialog
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

    private ContentPresenter? dialogButtons;

    public static readonly StyledProperty<MessageDialogButtons> ButtonsProperty = AvaloniaProperty.Register<MessageDialog, MessageDialogButtons>(nameof(Buttons));

    public static readonly StyledProperty<MessageDialogButtonsTemplate> ButtonsTemplateProperty = AvaloniaProperty.Register<MessageDialog, MessageDialogButtonsTemplate>(nameof(ButtonsTemplate), new MessageDialogButtonsTemplate());

    public static readonly StyledProperty<Brush> ButtonsBarBackgroundProperty = AvaloniaProperty.Register<MessageDialog, Brush>(nameof(ButtonsBarBackground));

    public static readonly StyledProperty<MessageDialogResult> DefaultResultProperty = AvaloniaProperty.Register<MessageDialog, MessageDialogResult>(nameof(DefaultResult));

    public MessageDialog()
    {
        DialogSize = DialogSize.Small;
        DefaultResult = MessageDialogResult.None;
    }

    public MessageDialogButtons Buttons
    {
        get { return GetValue(ButtonsProperty); }
        set { SetValue(ButtonsProperty, value); }
    }

    public MessageDialogButtonsTemplate ButtonsTemplate
    {
        get { return GetValue(ButtonsTemplateProperty); }
        set { SetValue(ButtonsTemplateProperty, value); }
    }

    public Brush ButtonsBarBackground
    {
        get { return GetValue(ButtonsBarBackgroundProperty); }
        set { SetValue(ButtonsBarBackgroundProperty, value); }
    }

    public MessageDialogResult DefaultResult
    {
        get { return GetValue(DefaultResultProperty); }
        set { SetValue(DefaultResultProperty, value); }
    }

    protected override Type StyleKeyOverride => typeof(MessageDialog);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (dialogButtons != null)
            dialogButtons.PropertyChanged -= ContentPresenter_ChildPropertyChanged;
        dialogButtons = e.NameScope.Get<ContentPresenter>("PART_DialogButtons");
        dialogButtons.PropertyChanged += ContentPresenter_ChildPropertyChanged;

        dialogButtons.ContentTemplate = ButtonsTemplate;
        dialogButtons.Content = this;
    }

    private void ContentPresenter_ChildPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
        {
            if (e.OldValue is ILogical oldChild)
                LogicalChildren.Remove(oldChild);
            if (e.NewValue is ILogical newLogical)
                LogicalChildren.Add(newLogical);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if ((change.Property == ButtonsTemplateProperty || change.Property == ButtonsProperty) && dialogButtons != null)
        {
            dialogButtons.ContentTemplate = ButtonsTemplate;
            dialogButtons.Content = this;
        }
    }

    #region ShowStatic

    public static async Task<MessageDialogResult> ShowDialog(string title, string text, MessageDialogButtons buttons, Window? parentWindow = null, bool forceOverlay = false)
    {
        var messageDialog = new MessageDialog { Title = title, Content = text, Buttons = buttons };
        var dialogTask = messageDialog.ShowDialog(parentWindow, forceOverlay);

        return (MessageDialogResult) (await dialogTask ?? messageDialog.DefaultResult);
    }

    public static async Task<MessageDialogResult> ShowDialog(string title, string text, MessageDialogButtons buttons, Page? parentPage, bool forceOverlay = false)
    {
        var messageDialog = new MessageDialog { Title = title, Content = text, Buttons = buttons };
        var dialogTask = messageDialog.ShowDialog(parentPage, forceOverlay);

        return (MessageDialogResult) (await dialogTask ?? messageDialog.DefaultResult);
    }

    public static async Task<MessageDialogResult> ShowDialogEmbedded(string title, string text, MessageDialogButtons buttons, ContentControl parentControl, bool forceOverlay = false)
    {
        var messageDialog = new MessageDialog { Title = title, Content = text, Buttons = buttons };
        var dialogTask = messageDialog.ShowDialogEmbedded(parentControl, forceOverlay);

        return (MessageDialogResult) (await dialogTask ?? messageDialog.DefaultResult);
    }

    #endregion
}