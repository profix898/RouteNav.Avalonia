using System;
using Avalonia;
using Avalonia.Layout;
using RouteNav.Avalonia.Internal;
using static System.Math;

namespace RouteNav.Avalonia.Dialogs;

/// <summary>Utility methods and defaults for sizing RouteNav dialogs.</summary>
public static class DialogSizeUtility
{
    #region DialigSizeDefaults

    /// <summary>Gets or sets the default scale used for small dialogs relative to the parent.</summary>
    public static Size SmallScale { get; set; } = new Size(0.3, 0.3);
    /// <summary>Gets or sets the default minimum size for small dialogs.</summary>
    public static Size SmallMinSize { get; set; } = new Size(200, 200);
    /// <summary>Gets or sets the default maximum size for small dialogs.</summary>
    public static Size SmallMaxSize { get; set; } = new Size(400, 400);
    
    /// <summary>Gets or sets the default scale used for medium dialogs relative to the parent.</summary>
    public static Size MediumScale { get; set; } = new Size(0.5, 0.5);
    /// <summary>Gets or sets the default minimum size for medium dialogs.</summary>
    public static Size MediumMinSize { get; set; } = new Size(350, 350);
    /// <summary>Gets or sets the default maximum size for medium dialogs.</summary>
    public static Size MediumMaxSize { get; set; } = new Size(700, 700);
    
    /// <summary>Gets or sets the default scale used for large dialogs relative to the parent.</summary>
    public static Size LargeScale { get; set; } = new Size(0.8, 0.8);
    /// <summary>Gets or sets the default minimum size for large dialogs.</summary>
    public static Size LargeMinSize { get; set; } = new Size(500, 500);
    /// <summary>Gets or sets the default maximum size for large dialogs.</summary>
    public static Size LargeMaxSize { get; set; } = new Size(1000, 1000);
    
    /// <summary>Gets or sets the size used when no parent size is available.</summary>
    public static Size FallbackSize { get; set; } = new Size(400, 300);

    #endregion

    /// <summary>Binds a dialog's width and height to a size calculated from the parent.</summary>
    public static IDisposable SetSizeBinding(this Dialog dialog, Layoutable parent, Size? minSize = null, Size? maxSize = null, DialogSize? dialogSize = null)
    {
        return Disposable.Create(dialog.Bind(Layoutable.WidthProperty, parent.GetBindingObservable(Layoutable.WidthProperty, _ => GetSize(dialog, parent, minSize, maxSize, dialogSize).Width)),
                                 dialog.Bind(Layoutable.HeightProperty, parent.GetBindingObservable(Layoutable.HeightProperty, _ => GetSize(dialog, parent, minSize, maxSize, dialogSize).Height)));
    }

    /// <summary>Sets a dialog's width and height from a size calculated from the parent.</summary>
    public static void SetSize(this Dialog dialog, Layoutable? parent, Size? minSize = null, Size? maxSize = null, DialogSize? dialogSize = null)
    {
        var size = GetSize(dialog, parent, minSize, maxSize, dialogSize);
        
        dialog.Width = size.Width;
        dialog.Height = size.Height;
    }

    /// <summary>Calculates the size for a dialog using the given parent and optional limits.</summary>
    public static Size GetSize(this Dialog dialog, Layoutable? parent, Size? minSize = null, Size? maxSize = null, DialogSize? dialogSize = null)
    {
        if (parent == null)
            return FallbackSize;

        var baseSize = GetBaseSize(parent);

        return (dialogSize ?? dialog.DialogSize) switch
        {
            DialogSize.Small => GetSize(baseSize, SmallScale, minSize ?? SmallMinSize, maxSize ?? SmallMaxSize),
            DialogSize.Medium => GetSize(baseSize, MediumScale, minSize ?? MediumMinSize, maxSize ?? MediumMaxSize),
            DialogSize.Large => GetSize(baseSize, LargeScale, minSize ?? LargeMinSize, maxSize ?? LargeMaxSize),
            DialogSize.Custom => (!Double.IsNaN(dialog.Width) && !Double.IsNaN(dialog.Height)) ? new Size(dialog.Width, dialog.Height) : GetSize(baseSize, new Size(0.5, 0.5)),
            _ => throw new ArgumentOutOfRangeException(nameof(dialog.DialogSize), dialog.DialogSize, null)
        };
    }

    #region Private

    private static Size GetBaseSize(Layoutable parent)
    {
        var baseSize = parent.Bounds.Size;

        // For RouteNav.Window use underlying platform window size
        if (parent is Window window && window.PlatformControl != null)
            baseSize = new Size(window.PlatformControl.Bounds.Width, window.PlatformControl.Bounds.Height);

        // For width/height is NaN use FallbackSize
        if (Double.IsNaN(baseSize.Width) || Double.IsNaN(baseSize.Height))
            baseSize = FallbackSize;

        return baseSize;
    }

    private static Size GetSize(Size baseSize, Size scale, Size? minSize = null, Size? maxSize = null)
    {
        var width = scale.Width * baseSize.Width;
        var height = scale.Height * baseSize.Height;

        // Limit minimum size
        if (minSize.HasValue)
        {
            width = Max(width, minSize.Value.Width);
            height = Max(height, minSize.Value.Height);
        }

        // Limit maximum size
        if (maxSize.HasValue)
        {
            width = Min(width, maxSize.Value.Width);
            height = Min(height, maxSize.Value.Height);
        }

        return new Size(Round(width), Round(height));
    }

    #endregion
}
