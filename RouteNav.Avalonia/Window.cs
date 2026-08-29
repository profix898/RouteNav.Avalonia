using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia;

/// <summary>
/// RouteNav's platform-agnostic window abstraction. It is hosted by a real desktop window or a single-view
/// platform control, exposed via <see cref="PlatformControl" />.
/// </summary>
public class Window : ContentControl
{
    /// <summary>
    /// Defines the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Window, string>(nameof(Title), "Window");

    /// <summary>
    /// Defines the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<WindowIcon> IconProperty = AvaloniaProperty.Register<Window, WindowIcon>(nameof(Icon));

    /// <summary>Initializes a new instance of the <see cref="Window" /> class.</summary>
    public Window()
        : this(null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Window" /> class with the given content.</summary>
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

    #region Events

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

    /// <summary>
    /// Sets the content of the navigation surface. When the RouteNav window itself is hosted as
    /// the platform control's content (desktop/single-view hosting), the content is set on the
    /// RouteNav window so its window-level XAML (DynamicResource backgrounds, styles, resources)
    /// keeps resolving against the application resource chain.
    /// </summary>
    /// <exception cref="NavigationException">Thrown when there is no backing platform control.</exception>
    public void SetContent(Control content)
    {
        if (PlatformControl != null)
        {
            if (ReferenceEquals(PlatformControl.Content, this))
                Content = content;
            else
                PlatformControl.Content = content;
        }
        else
            throw new NavigationException("Main window/view does not have a backing platform control.");
    }

    /// <summary>Closes the window (desktop) or shuts down the application where applicable.</summary>
    public void Close()
    {
        if (PlatformControl is AvaloniaWindow window)
            window.Close();
        else if (ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime)
            desktopLifetime.TryShutdown();

        // Note: We can't shutdown other lifetimes (e.g. on mobile) -> ignore the request
    }

    #region Platform

    /// <summary>
    /// Gets the associated application lifetime
    /// </summary>
    public IApplicationLifetime? ApplicationLifetime { get; private set; }

    /// <summary>
    /// Gets the associated platform control
    /// </summary>
    public ContentControl? PlatformControl { get; private set; }

    internal void RegisterPlatform(IApplicationLifetime appLifetime, ContentControl platformControl)
    {
        ApplicationLifetime = appLifetime;
        PlatformControl = platformControl;

        if (platformControl is TopLevel topLevel)
        {
            topLevel.Opened += (_, _) => OnOpened();
            topLevel.Closed += (_, _) => OnClosed();
        }
        else
            OnOpened();
    }

    #endregion
}
