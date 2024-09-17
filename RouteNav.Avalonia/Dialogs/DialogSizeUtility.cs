using System;
using Avalonia;
using Avalonia.Layout;
using RouteNav.Avalonia.Internal;
using static System.Math;

namespace RouteNav.Avalonia.Dialogs;

public static class DialogSizeUtility
{
    #region DialigSizeDefaults

    public static Size SmallScale { get; set; } = new Size(0.3, 0.3);
    public static Size SmallMinSize { get; set; } = new Size(200, 200);
    public static Size SmallMaxSize { get; set; } = new Size(400, 400);
    
    public static Size MediumScale { get; set; } = new Size(0.6, 0.6);
    public static Size MediumMinSize { get; set; } = new Size(350, 350);
    public static Size MediumMaxSize { get; set; } = new Size(700, 700);
    
    public static Size LargeScale { get; set; } = new Size(0.9, 0.9);
    public static Size LargeMinSize { get; set; } = new Size(500, 500);
    public static Size LargeMaxSize { get; set; } = new Size(1000, 1000);
    
    public static Size FallbackSize { get; set; } = new Size(400, 300);

    #endregion

    public static IDisposable SetSizeBinding(this Dialog dialog, Layoutable parent, Size? minSize = null, Size? maxSize = null)
    {
        return Disposable.Create(dialog.Bind(Layoutable.WidthProperty, parent.GetBindingObservable(Layoutable.WidthProperty, _ => GetSize(dialog, parent, minSize, maxSize).Width)),
                                 dialog.Bind(Layoutable.HeightProperty, parent.GetBindingObservable(Layoutable.HeightProperty, _ => GetSize(dialog, parent, minSize, maxSize).Height)));
    }

    public static void SetSize(this Dialog dialog, Layoutable? parent, Size? minSize = null, Size? maxSize = null)
    {
        var size = GetSize(dialog, parent, minSize, maxSize);
        
        dialog.Width = size.Width;
        dialog.Height = size.Height;
    }

    public static Size GetSize(Dialog dialog, Layoutable? parent, Size? minSize = null, Size? maxSize = null)
    {
        if (parent == null)
            return FallbackSize;

        var baseSize = GetBaseSize(parent);

        return dialog.DialogSize switch
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
