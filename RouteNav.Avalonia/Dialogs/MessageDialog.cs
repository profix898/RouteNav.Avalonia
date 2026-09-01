using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace RouteNav.Avalonia.Dialogs;

/// <summary>A <see cref="Dialog" /> that presents a message with a configurable set of buttons and returns a <see cref="MessageDialogResult" />.</summary>
public class MessageDialog : Dialog
{
    private ContentPresenter? dialogButtons;
    private Panel? dialogButtonBar;

    /// <summary>Defines the <see cref="Buttons" /> property.</summary>
    public static readonly StyledProperty<MessageDialogButtons> ButtonsProperty = AvaloniaProperty.Register<MessageDialog, MessageDialogButtons>(nameof(Buttons));

    /// <summary>Defines the <see cref="ButtonsTemplate" /> property.</summary>
    public static readonly StyledProperty<MessageDialogButtonsTemplate> ButtonsTemplateProperty =
        AvaloniaProperty.Register<MessageDialog, MessageDialogButtonsTemplate>(nameof(ButtonsTemplate), new MessageDialogButtonsTemplate());

    /// <summary>Defines the <see cref="ButtonsBarBackground" /> property.</summary>
    public static readonly StyledProperty<Brush> ButtonsBarBackgroundProperty = AvaloniaProperty.Register<MessageDialog, Brush>(nameof(ButtonsBarBackground));

    /// <summary>Defines the <see cref="DefaultResult" /> property.</summary>
    public static readonly StyledProperty<MessageDialogResult> DefaultResultProperty = AvaloniaProperty.Register<MessageDialog, MessageDialogResult>(nameof(DefaultResult));

    /// <summary>Initializes a new instance of the <see cref="MessageDialog" /> class.</summary>
    public MessageDialog()
    {
        // Fixed width, content-hugging height: the frame fits the message
        DialogSize = DialogSize.Custom;
        Width = 400;
        DefaultResult = MessageDialogResult.None;

        HorizontalContentAlignment = HorizontalAlignment.Center;
        VerticalContentAlignment = VerticalAlignment.Center;
    }

    /// <summary>Gets or sets the button set shown by the dialog.</summary>
    public MessageDialogButtons Buttons
    {
        get { return GetValue(ButtonsProperty); }
        set { SetValue(ButtonsProperty, value); }
    }

    /// <summary>Gets or sets the template used to build the buttons.</summary>
    public MessageDialogButtonsTemplate ButtonsTemplate
    {
        get { return GetValue(ButtonsTemplateProperty); }
        set { SetValue(ButtonsTemplateProperty, value); }
    }

    /// <summary>Gets or sets the background brush of the buttons bar.</summary>
    public Brush ButtonsBarBackground
    {
        get { return GetValue(ButtonsBarBackgroundProperty); }
        set { SetValue(ButtonsBarBackgroundProperty, value); }
    }

    /// <summary>Gets or sets the result returned when the dialog is dismissed without an explicit choice.</summary>
    public MessageDialogResult DefaultResult
    {
        get { return GetValue(DefaultResultProperty); }
        set { SetValue(DefaultResultProperty, value); }
    }

    /// <summary>Sets the dialog content to a plain text block with the given text.</summary>
    public string TextContent
    {
        set { SetValue(ContentProperty, new TextBlock { Text = value, TextWrapping = TextWrapping.Wrap }); }
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(MessageDialog);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (dialogButtons != null)
            dialogButtons.PropertyChanged -= ContentPresenter_ChildPropertyChanged;
        dialogButtons = e.NameScope.Get<ContentPresenter>("MessageDialogButtons");
        dialogButtons.PropertyChanged += ContentPresenter_ChildPropertyChanged;
        dialogButtonBar = e.NameScope.Find<Panel>("DialogButtonBar");

        dialogButtons.ContentTemplate = ButtonsTemplate;
        dialogButtons.Content = this;

        UpdateContentScrollViewerMaxHeight();
    }

    /// <inheritdoc />
    protected override void UpdateContentScrollViewerMaxHeight()
    {
        if (dialogContentScrollViewer == null)
            return;

        // Wrap mode and content-hugging dialogs host unconstrained content: the frame grows with it
        if (DialogContentMode == DialogContentMode.Wrap || Double.IsNaN(Height))
        {
            dialogContentScrollViewer.MaxHeight = Double.PositiveInfinity;
            return;
        }

        var frameHeight = GetConstrainedHeight();
        if (frameHeight <= 0 || Double.IsInfinity(frameHeight))
            return;

        var titleBarHeight = GetVisibleHeight(dialogTitleBarPanel);
        var buttonBarHeight = GetVisibleHeight(dialogButtonBar);

        dialogContentScrollViewer.MaxHeight = Math.Max(0, frameHeight - titleBarHeight - buttonBarHeight);
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

    /// <inheritdoc />
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

    /// <summary>Creates a message dialog with plain text content.</summary>
    public static MessageDialog Create(string title, string text, MessageDialogButtons buttons)
    {
        return new MessageDialog { Title = title, TextContent = text, Buttons = buttons };
    }

    /// <summary>Creates a message dialog with custom content.</summary>
    public static MessageDialog Create(string title, object content, MessageDialogButtons buttons)
    {
        return new MessageDialog { Title = title, Content = content, Buttons = buttons };
    }

    #endregion
}
