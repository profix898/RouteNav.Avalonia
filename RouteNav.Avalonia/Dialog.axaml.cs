using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DialogHostAvalonia;
using System;

namespace NSE.RouteNav;

public partial class Dialog : Page
{
    private bool closeOnClickAway;

    public static readonly StyledProperty<Brush> TitleBarBrushProperty = AvaloniaProperty.Register<Dialog, Brush>(nameof(TitleBarBrush));

    public static readonly StyledProperty<Brush> TitleTextBrushProperty = AvaloniaProperty.Register<Dialog, Brush>(nameof(TitleTextBrush));

    public static readonly StyledProperty<bool> TitleBarVisibleProperty = AvaloniaProperty.Register<Dialog, bool>(nameof(TitleBarVisible));

    public static readonly StyledProperty<Size> SizeProperty = AvaloniaProperty.Register<Dialog, Size>(nameof(Size));

    public static readonly DirectProperty<Dialog, bool> CloseOnClickAwayProperty = AvaloniaProperty.RegisterDirect(nameof(CloseOnClickAway), (Func<Dialog, bool>) (o => o.CloseOnClickAway), (Action<Dialog, bool>) ((o, v) => o.CloseOnClickAway = v));

    public Dialog()
    {
        AvaloniaXamlLoader.Load(this);

        DataContext = this;
    }

    public Brush TitleBarBrush
    {
        get { return GetValue(TitleBarBrushProperty); }
        set { SetValue(TitleBarBrushProperty, value); }
    }

    public Brush TitleTextBrush
    {
        get { return GetValue(TitleTextBrushProperty); }
        set { SetValue(TitleTextBrushProperty, value); }
    }

    public bool TitleBarVisible
    {
        get { return GetValue(TitleBarVisibleProperty); }
        set { SetValue(TitleBarVisibleProperty, value); }
    }

    public Size Size
    {
        get { return GetValue(SizeProperty); }
        set { SetValue(SizeProperty, value); }
    }

    public bool CloseOnClickAway
    {
        get { return closeOnClickAway; }
        set { SetAndRaise(DialogHost.CloseOnClickAwayProperty, ref closeOnClickAway, value); }
    }

    /// <summary>
    /// Fired when the window is opened.
    /// </summary>
    public event EventHandler? Opened;

    internal void OnOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Fired when the window is closed.
    /// </summary>
    public event EventHandler? Closed;

    internal void OnClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
