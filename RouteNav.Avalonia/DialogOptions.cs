using System;
using Avalonia;

namespace RouteNav.Avalonia;

/// <summary>Dialog options, available via <see cref="Navigation.Dialogs" />.</summary>
public class DialogOptions
{
    /// <summary>Gets or sets the default scale used for small dialogs relative to the parent.</summary>
    public Size SmallScale { get; set; } = new Size(0.3, 0.3);

    /// <summary>Gets or sets the default minimum size for small dialogs.</summary>
    public Size SmallMinSize { get; set; } = new Size(200, 200);

    /// <summary>Gets or sets the default maximum size for small dialogs.</summary>
    public Size SmallMaxSize { get; set; } = new Size(400, 400);

    /// <summary>Gets or sets the default scale used for medium dialogs relative to the parent.</summary>
    public Size MediumScale { get; set; } = new Size(0.5, 0.5);

    /// <summary>Gets or sets the default minimum size for medium dialogs.</summary>
    public Size MediumMinSize { get; set; } = new Size(350, 350);

    /// <summary>Gets or sets the default maximum size for medium dialogs.</summary>
    public Size MediumMaxSize { get; set; } = new Size(700, 700);

    /// <summary>Gets or sets the default scale used for large dialogs relative to the parent.</summary>
    public Size LargeScale { get; set; } = new Size(0.8, 0.8);

    /// <summary>Gets or sets the default minimum size for large dialogs.</summary>
    public Size LargeMinSize { get; set; } = new Size(500, 500);

    /// <summary>Gets or sets the default maximum size for large dialogs.</summary>
    public Size LargeMaxSize { get; set; } = new Size(1000, 1000);

    /// <summary>Gets or sets the size used when no parent size is available.</summary>
    public Size FallbackSize { get; set; } = new Size(400, 300);

    /// <summary>Gets or sets the duration of the overlay dialog close animation (also delays the dialog's Closed event).</summary>
    public TimeSpan OverlayCloseAnimationDuration { get; set; } = TimeSpan.FromMilliseconds(180);
}
