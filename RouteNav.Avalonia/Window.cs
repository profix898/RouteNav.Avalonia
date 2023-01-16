using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DialogHostAvalonia;
using NSE.RouteNav.Bootstrap;
using NSE.RouteNav.Platform;

namespace NSE.RouteNav;

public class Window : ContentControl
{
    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Window, string>(nameof(Title), "Window");

    /// <summary>
    /// Defines the <see cref="Icon"/> property.
    /// </summary>
    public static readonly StyledProperty<WindowIcon> IconProperty = AvaloniaProperty.Register<Window, WindowIcon>(nameof(Icon));

    public Window()
        : this(null)
    {
    }

    public Window(object? content)
    {
        if (content != null)
            Content = content;
    }

    /// <summary>
    /// Gets or sets the title of the window
    /// </summary>
    public string Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    /// <summary>
    /// Gets or sets the icon of the window
    /// </summary>
    public WindowIcon Icon
    {
        get { return GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    #region WindowContent

    public void SetContent(object content)
    {
        if (DialogHost == null)
            throw new InvalidOperationException("Can't set or update window content before platform window/view was created.");

        DialogHost.Content = content;
    }

    /// <summary>
    /// Fired when the window is opened
    /// </summary>
    public event EventHandler? Opened;

    internal void OnOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Fired when the window is closed
    /// </summary>
    public event EventHandler? Closed;

    internal void OnClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region DialogContent

    public void SetDialogContent(object content)
    {
        if (DialogHost == null)
            throw new InvalidOperationException("Can't set or update dialog content before platform window/view was created.");

        DialogHost.DialogContent = content;
    }

    /// <summary>
    /// Fired when the window is opened
    /// </summary>
    public event EventHandler? DialogOpened;

    internal void OnDialogOpened()
    {
        DialogOpened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Fired when the window is closed
    /// </summary>
    public event EventHandler? DialogClosed;

    internal void OnDialogClosed()
    {
        DialogClosed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Gets the associated platform window
    /// </summary>
    public Avalonia.Controls.Window? PlatformWindow { get; private set; }

    /// <summary>
    /// Gets the associated application lifetime
    /// </summary>
    public IApplicationLifetime? ApplicationLifetime { get; private set; }

    /// <summary>
    /// Informs wether the window represents a dialog window (overlay) or a standard platform window/view (with embedded dialog host)
    /// </summary>
    public bool IsDialogWindow => (DialogHost == null);

    /// <summary>
    /// Gets the DialogHost for this window (after ToPlatformView/Window conversion)
    /// </summary>
    public DialogHost? DialogHost { get; private set; }

    #endregion

    #region Platform

    public Avalonia.Controls.Window ToPlatformWindow(IClassicDesktopStyleApplicationLifetime desktopLifetime, Action<Avalonia.Controls.Window>? windowCustomization = null)
    {
        PlatformWindow = new Avalonia.Controls.Window
        {
            Title = Title,
            Icon = Icon,

            // ContentControl
            Content = DialogHost = new DialogHost
            {
                Content = Content,
                ContentTemplate = ContentTemplate,
                CloseOnClickAway = true
            },
            HorizontalContentAlignment = HorizontalContentAlignment,
            VerticalContentAlignment = VerticalContentAlignment,

            // TemplatedControl
            Background = Background,
            BorderBrush = BorderBrush,
            BorderThickness = BorderThickness,
            CornerRadius = CornerRadius,
            FontFamily = FontFamily,
            FontSize = FontSize,
            FontStyle = FontStyle,
            FontWeight = FontWeight,
            FontStretch = FontStretch,
            Foreground = Foreground,
            Padding = Padding,

            // Control
            FocusAdorner = FocusAdorner,
            Tag = this,
            ContextMenu = ContextMenu,
            ContextFlyout = ContextFlyout
        };
#if DEBUG
        PlatformWindow.AttachDevTools();
#endif
        PlatformWindow.Opened += (_, _) => OnOpened();
        PlatformWindow.Closed += (_, _) => OnClosed();
        windowCustomization?.Invoke(PlatformWindow);
        ApplicationLifetime = desktopLifetime;

        return PlatformWindow;
    }

    public Control ToPlatformView(ISingleViewApplicationLifetime singleViewLifetime)
    {
        PlatformWindow = null;
        ApplicationLifetime = singleViewLifetime;

        return DialogHost = new DialogHost
        {
            Content = Content,
            ContentTemplate = ContentTemplate,
            CloseOnClickAway = true,

            // TemplatedControl
            Background = Background,
            BorderBrush = BorderBrush,
            BorderThickness = BorderThickness,
            CornerRadius = CornerRadius,
            FontFamily = FontFamily,
            FontSize = FontSize,
            FontStyle = FontStyle,
            FontWeight = FontWeight,
            FontStretch = FontStretch,
            Foreground = Foreground,
            Padding = Padding,

            // Control
            FocusAdorner = FocusAdorner,
            Tag = this,
            ContextMenu = ContextMenu,
            ContextFlyout = ContextFlyout
        };
    }

    #endregion
}
