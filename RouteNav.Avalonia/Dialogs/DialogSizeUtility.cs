using System;
using Avalonia;
using Avalonia.Layout;
using static System.Math;

namespace RouteNav.Avalonia.Dialogs;

public static class DialogSizeUtility
{
    public static readonly Size DefaultSize = new Size(400, 300);

    public static void SetSize(this Dialog dialog, Layoutable? parent, Size? minSize = null, Size? maxSize = null)
    {
        var baseSize = (parent != null) ? GetBaseSize(parent) : DefaultSize;
        var size = dialog.DialogSize switch
        {
            DialogSize.Small => GetSize(baseSize, new Size(0.3, 0.3), minSize ?? new Size(200, 200), maxSize ?? new Size(400, 400)),
            DialogSize.Medium => GetSize(baseSize, new Size(0.6, 0.6), minSize ?? new Size(350, 350), maxSize ?? new Size(700, 700)),
            DialogSize.Large => GetSize(baseSize, new Size(0.9, 0.9), minSize ?? new Size(500, 500), maxSize ?? new Size(1000, 1000)),
            DialogSize.Custom => new Size(dialog.Width, dialog.Height),
            _ => throw new ArgumentOutOfRangeException(nameof(dialog.DialogSize), dialog.DialogSize, null)
        };

        dialog.Width = Round(size.Width);
        dialog.Height = Round(size.Height);
    }

    public static Size GetSize(DialogSize dialogSize, Layoutable? parent, Size? minSize = null, Size? maxSize = null)
    {
        if (parent == null)
            return DefaultSize;

        var baseSize = GetBaseSize(parent);

        return dialogSize switch
        {
            DialogSize.Small => GetSize(baseSize, new Size(0.3, 0.3), minSize ?? new Size(200, 200), maxSize ?? new Size(400, 400)),
            DialogSize.Medium => GetSize(baseSize, new Size(0.6, 0.6), minSize ?? new Size(350, 350), maxSize ?? new Size(700, 700)),
            DialogSize.Large => GetSize(baseSize, new Size(0.9, 0.9), minSize ?? new Size(500, 500), maxSize ?? new Size(1000, 1000)),
            DialogSize.Custom => parent.Bounds.Size,
            _ => throw new ArgumentOutOfRangeException(nameof(dialogSize), dialogSize, null)
        };
    }

    #region Private

    private static Size GetBaseSize(Layoutable parent)
    {
        var baseSize = parent.Bounds.Size;

        // For RouteNav.Window use underlying platform window size
        if (parent is Window window && window.PlatformControl != null)
            baseSize = new Size(window.PlatformControl.Bounds.Width, window.PlatformControl.Bounds.Height);

        // For width/height is NaN use DefaultSize
        if (Double.IsNaN(baseSize.Width) || Double.IsNaN(baseSize.Height))
            baseSize = DefaultSize;

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

        return new Size(width, height);
    }

    #endregion
}
