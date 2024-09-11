using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace RouteNav.Avalonia.Dialogs;

public class MessageDialog : Dialog
{
    private ContentPresenter? dialogButtons;

    public static readonly StyledProperty<MessageDialogButtons> ButtonsProperty = AvaloniaProperty.Register<MessageDialog, MessageDialogButtons>(nameof(Buttons));

    public static readonly StyledProperty<MessageDialogButtonsTemplate> ButtonsTemplateProperty = AvaloniaProperty.Register<MessageDialog, MessageDialogButtonsTemplate>(nameof(ButtonsTemplate), new MessageDialogButtonsTemplate());

    public static readonly StyledProperty<Brush> ButtonsBarBackgroundProperty = AvaloniaProperty.Register<MessageDialog, Brush>(nameof(ButtonsBarBackground));

    public static readonly StyledProperty<MessageDialogResult> DefaultResultProperty = AvaloniaProperty.Register<MessageDialog, MessageDialogResult>(nameof(DefaultResult));

    public MessageDialog()
    {
        DialogSize = DialogSize.Small;
        DefaultResult = MessageDialogResult.None;
        
        HorizontalContentAlignment = HorizontalAlignment.Center;
        VerticalContentAlignment = VerticalAlignment.Center;
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

    public string TextContent
    {
        set { SetValue(ContentProperty, new TextBlock { Text = value }); }
    }

    protected override Type StyleKeyOverride => typeof(MessageDialog);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (dialogButtons != null)
            dialogButtons.PropertyChanged -= ContentPresenter_ChildPropertyChanged;
        dialogButtons = e.NameScope.Get<ContentPresenter>("MessageDialogButtons");
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

    #region Factory

    public static MessageDialog Create(string title, string text, MessageDialogButtons buttons)
    {
        return new MessageDialog { Title = title, TextContent = text, Buttons = buttons };
    }

    public static MessageDialog Create(string title, object content, MessageDialogButtons buttons)
    {
        return new MessageDialog { Title = title, Content = content, Buttons = buttons };
    }

    #endregion
}