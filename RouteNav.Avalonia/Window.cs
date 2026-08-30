using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
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

    #region Window chrome

    // Declarative OS-window chrome: these properties are mirrored onto the platform window when it
    // is created (desktop), so window shells can declare them in XAML. Anything not mirrored here
    // can be customized through the window manager's WindowCustomizationEvent.

    /// <summary>Defines the <see cref="CanResize" /> property.</summary>
    public static readonly StyledProperty<bool> CanResizeProperty = AvaloniaProperty.Register<Window, bool>(nameof(CanResize), true);

    /// <summary>Defines the <see cref="CanMinimize" /> property.</summary>
    public static readonly StyledProperty<bool> CanMinimizeProperty = AvaloniaProperty.Register<Window, bool>(nameof(CanMinimize), true);

    /// <summary>Defines the <see cref="CanMaximize" /> property.</summary>
    public static readonly StyledProperty<bool> CanMaximizeProperty = AvaloniaProperty.Register<Window, bool>(nameof(CanMaximize), true);

    /// <summary>Defines the <see cref="ShowInTaskbar" /> property.</summary>
    public static readonly StyledProperty<bool> ShowInTaskbarProperty = AvaloniaProperty.Register<Window, bool>(nameof(ShowInTaskbar), true);

    /// <summary>Defines the <see cref="ShowActivated" /> property.</summary>
    public static readonly StyledProperty<bool> ShowActivatedProperty = AvaloniaProperty.Register<Window, bool>(nameof(ShowActivated), true);

    /// <summary>Defines the <see cref="Topmost" /> property.</summary>
    public static readonly StyledProperty<bool> TopmostProperty = AvaloniaProperty.Register<Window, bool>(nameof(Topmost));

    /// <summary>Defines the <see cref="WindowState" /> property.</summary>
    public static readonly StyledProperty<WindowState> WindowStateProperty = AvaloniaProperty.Register<Window, WindowState>(nameof(WindowState));

    /// <summary>Defines the <see cref="WindowStartupLocation" /> property.</summary>
    public static readonly StyledProperty<WindowStartupLocation> WindowStartupLocationProperty =
        AvaloniaProperty.Register<Window, WindowStartupLocation>(nameof(WindowStartupLocation));

    /// <summary>Defines the <see cref="WindowDecorations" /> property.</summary>
    public static readonly StyledProperty<WindowDecorations> WindowDecorationsProperty =
        AvaloniaProperty.Register<Window, WindowDecorations>(nameof(WindowDecorations), WindowDecorations.Full);

    /// <summary>Defines the <see cref="ExtendClientAreaToDecorationsHint" /> property.</summary>
    public static readonly StyledProperty<bool> ExtendClientAreaToDecorationsHintProperty = AvaloniaProperty.Register<Window, bool>(nameof(ExtendClientAreaToDecorationsHint));

    /// <summary>Defines the <see cref="TransparencyLevelHint" /> property.</summary>
    public static readonly StyledProperty<IReadOnlyList<WindowTransparencyLevel>> TransparencyLevelHintProperty =
        AvaloniaProperty.Register<Window, IReadOnlyList<WindowTransparencyLevel>>(nameof(TransparencyLevelHint));

    /// <summary>Defines the <see cref="TransparencyBackgroundFallback" /> property.</summary>
    public static readonly StyledProperty<IBrush> TransparencyBackgroundFallbackProperty = AvaloniaProperty.Register<Window, IBrush>(nameof(TransparencyBackgroundFallback));

    /// <summary>Gets or sets a value indicating whether the user can resize the window.</summary>
    public bool CanResize
    {
        get { return GetValue(CanResizeProperty); }
        set { SetValue(CanResizeProperty, value); }
    }

    /// <summary>Gets or sets a value indicating whether the minimize button is enabled.</summary>
    public bool CanMinimize
    {
        get { return GetValue(CanMinimizeProperty); }
        set { SetValue(CanMinimizeProperty, value); }
    }

    /// <summary>Gets or sets a value indicating whether the maximize button is enabled.</summary>
    public bool CanMaximize
    {
        get { return GetValue(CanMaximizeProperty); }
        set { SetValue(CanMaximizeProperty, value); }
    }

    /// <summary>Gets or sets a value indicating whether the window is shown in the OS taskbar.</summary>
    public bool ShowInTaskbar
    {
        get { return GetValue(ShowInTaskbarProperty); }
        set { SetValue(ShowInTaskbarProperty, value); }
    }

    /// <summary>Gets or sets a value indicating whether the window is activated when first shown.</summary>
    public bool ShowActivated
    {
        get { return GetValue(ShowActivatedProperty); }
        set { SetValue(ShowActivatedProperty, value); }
    }

    /// <summary>Gets or sets a value indicating whether the window stays above all other windows.</summary>
    public bool Topmost
    {
        get { return GetValue(TopmostProperty); }
        set { SetValue(TopmostProperty, value); }
    }

    /// <summary>Gets or sets the initial window state (normal, minimized, maximized or full screen).</summary>
    public WindowState WindowState
    {
        get { return GetValue(WindowStateProperty); }
        set { SetValue(WindowStateProperty, value); }
    }

    /// <summary>Gets or sets the startup location of the window (manual, centered on screen or on the owner).</summary>
    public WindowStartupLocation WindowStartupLocation
    {
        get { return GetValue(WindowStartupLocationProperty); }
        set { SetValue(WindowStartupLocationProperty, value); }
    }

    /// <summary>Gets or sets the title bar and border style of the window.</summary>
    public WindowDecorations WindowDecorations
    {
        get { return GetValue(WindowDecorationsProperty); }
        set { SetValue(WindowDecorationsProperty, value); }
    }

    /// <summary>Gets or sets a value indicating whether the client area extends into the title bar area (for custom chrome).</summary>
    public bool ExtendClientAreaToDecorationsHint
    {
        get { return GetValue(ExtendClientAreaToDecorationsHintProperty); }
        set { SetValue(ExtendClientAreaToDecorationsHintProperty, value); }
    }

    /// <summary>Gets or sets the requested window transparency level(s).</summary>
    public IReadOnlyList<WindowTransparencyLevel> TransparencyLevelHint
    {
        get { return GetValue(TransparencyLevelHintProperty); }
        set { SetValue(TransparencyLevelHintProperty, value); }
    }

    /// <summary>Gets or sets the fallback background brush used when transparency is unavailable.</summary>
    public IBrush TransparencyBackgroundFallback
    {
        get { return GetValue(TransparencyBackgroundFallbackProperty); }
        set { SetValue(TransparencyBackgroundFallbackProperty, value); }
    }

    #endregion

    #region Platform

    /// <summary>Gets the associated application lifetime</summary>
    public IApplicationLifetime? ApplicationLifetime { get; private set; }

    /// <summary>Gets the associated platform control</summary>
    public ContentControl? PlatformControl { get; private set; }

    /// <summary>Gets a value indicating whether the window was closed. A closed window can be re-opened by navigating to its stack.</summary>
    public bool IsClosed { get; private set; }

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
        IsClosed = true;
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
        if (PlatformControl == null)
            throw new NavigationException("Main window/view does not have a backing platform control.");

        SetContentCore(content);
    }

    /// <summary>
    /// Routes navigation content into the window shell. Windows with custom chrome (e.g. a toolbar
    /// around the navigation surface) override this method to place the content in a dedicated
    /// content host instead of the shell root.
    /// </summary>
    protected virtual void SetContentCore(Control content)
    {
        if (PlatformControl != null && !ReferenceEquals(PlatformControl.Content, this))
            PlatformControl.Content = content;
        else
            Content = content;
    }

    /// <summary>Applies the initial content before the window is hosted (via <see cref="WindowFactory" />).</summary>
    internal void SetInitialContent(Control content) => SetContentCore(content);

    /// <summary>Closes the window (desktop) or shuts down the application where applicable.</summary>
    public void Close()
    {
        if (PlatformControl is AvaloniaWindow window)
            window.Close();
        else if (ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktopLifetime)
            desktopLifetime.TryShutdown();

        // Note: We can't shutdown other lifetimes (e.g. on mobile) -> ignore the request
    }

    /// <summary>
    /// Brings the window to the foreground. Desktop platforms only; single-view platforms have no
    /// window z-order, so the call is ignored there.
    /// </summary>
    public void Activate()
    {
        if (PlatformControl is not AvaloniaWindow platformWindow)
            return;

        // Restore a minimized window first (SetForegroundWindow does not restore it).
        if (platformWindow.WindowState == WindowState.Minimized)
            platformWindow.WindowState = WindowState.Normal;

        // Defer activation until the current input processing has finished: Windows may reject
        // SetForegroundWindow while the click that triggered this call is still being handled
        // (foreground lock), leaving the window flashing in the taskbar instead of raising it.
        Dispatcher.UIThread.Post(platformWindow.Activate);
    }
}
