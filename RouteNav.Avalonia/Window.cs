using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace RouteNav.Avalonia;

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

    public void SetContent(Control content)
    {
        if (PlatformControl != null)
            PlatformControl.Content = content;
        else
            throw new NavigationException("Main window/view does not have a backing platform control.");
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

    public void RegisterPlatform(IApplicationLifetime appLifetime, ContentControl platformControl)
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
