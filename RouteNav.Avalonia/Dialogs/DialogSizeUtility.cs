using System;
using Avalonia;
using Avalonia.Layout;
using RouteNav.Avalonia.Internal;
using static System.Math;

namespace RouteNav.Avalonia.Dialogs;

/// <summary>Utility methods for sizing RouteNav dialogs. Sizing defaults are configurable via <see cref="Navigation.Dialogs" />.</summary>
public static class DialogSizeUtility
{
    /// <summary>Binds a dialog's width and height to a size calculated from the parent.</summary>
    public static IDisposable SetSizeBinding(this Dialog dialog, Layoutable parent, Size? minSize = null, Size? maxSize = null, DialogSize? dialogSize = null)
    {
        return Disposable.Create(dialog.Bind(Layoutable.WidthProperty,
                                             parent.GetBindingObservable(Layoutable.WidthProperty, _ => dialog.GetSize(parent, minSize, maxSize, dialogSize).Width)),
                                 dialog.Bind(Layoutable.HeightProperty,
                                             parent.GetBindingObservable(Layoutable.HeightProperty, _ => dialog.GetSize(parent, minSize, maxSize, dialogSize).Height)));
    }

    /// <summary>Sets a dialog's width and height from a size calculated from the parent.</summary>
    public static void SetSize(this Dialog dialog, Layoutable? parent, Size? minSize = null, Size? maxSize = null, DialogSize? dialogSize = null)
    {
        var size = dialog.GetSize(parent, minSize, maxSize, dialogSize);

        dialog.Width = size.Width;
        dialog.Height = size.Height;
    }

    /// <summary>Calculates the size for a dialog using the given parent and optional limits.</summary>
    public static Size GetSize(this Dialog dialog, Layoutable? parent, Size? minSize = null, Size? maxSize = null, DialogSize? dialogSize = null)
    {
        var options = Navigation.Dialogs;

        if (parent == null)
            return options.FallbackSize;

        var baseSize = GetBaseSize(parent);

        return (dialogSize ?? dialog.DialogSize) switch
        {
            DialogSize.Small => GetSize(baseSize, options.SmallScale, minSize ?? options.SmallMinSize, maxSize ?? options.SmallMaxSize),
            DialogSize.Medium => GetSize(baseSize, options.MediumScale, minSize ?? options.MediumMinSize, maxSize ?? options.MediumMaxSize),
            DialogSize.Large => GetSize(baseSize, options.LargeScale, minSize ?? options.LargeMinSize, maxSize ?? options.LargeMaxSize),
            DialogSize.Custom => !Double.IsNaN(dialog.Width) && !Double.IsNaN(dialog.Height) ? new Size(dialog.Width, dialog.Height) : GetSize(baseSize, new Size(0.5, 0.5)),
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
            baseSize = Navigation.Dialogs.FallbackSize;

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
