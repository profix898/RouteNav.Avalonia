using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Layout;
using RouteNav.Avalonia.Internal;
using static System.Math;

namespace RouteNav.Avalonia.Dialogs;

/// <summary>Utility methods for sizing RouteNav dialogs. Sizing defaults are configurable via <see cref="Navigation.Dialogs" />.</summary>
public static class DialogSizeUtility
{
    /// <summary>
    /// Binds a dialog's size to its parent. Only axes that the dialog leaves unset and that receive a
    /// concrete value from the size calculation are bound; explicitly set axes keep their value and
    /// content-hug axes (resolved to NaN) stay content-sized.
    /// </summary>
    public static IDisposable SetSizeBinding(this Dialog dialog, Layoutable parent, Size? minSize = null, Size? maxSize = null, DialogSize? dialogSize = null)
    {
        var size = dialog.GetSize(parent, minSize, maxSize, dialogSize);
        var bindings = new List<IDisposable>(2);

        if (Double.IsNaN(dialog.Width) && !Double.IsNaN(size.Width))
        {
            dialog.IsWidthSystemAssigned = true;
            bindings.Add(dialog.Bind(Layoutable.WidthProperty,
                                     parent.GetBindingObservable(Layoutable.WidthProperty, _ => dialog.GetSize(parent, minSize, maxSize, dialogSize).Width)));
        }

        if (Double.IsNaN(dialog.Height) && !Double.IsNaN(size.Height))
        {
            dialog.IsHeightSystemAssigned = true;
            bindings.Add(dialog.Bind(Layoutable.HeightProperty,
                                     parent.GetBindingObservable(Layoutable.HeightProperty, _ => dialog.GetSize(parent, minSize, maxSize, dialogSize).Height)));
        }

        return Disposable.Create(bindings.ToArray());
    }

    /// <summary>Sets a dialog's size from a size calculated from the parent. Explicitly set axes are kept.</summary>
    public static void SetSize(this Dialog dialog, Layoutable? parent, Size? minSize = null, Size? maxSize = null, DialogSize? dialogSize = null)
    {
        var size = dialog.GetSize(parent, minSize, maxSize, dialogSize);

        if (Double.IsNaN(dialog.Width) && !Double.IsNaN(size.Width))
        {
            dialog.Width = size.Width;
            dialog.IsWidthSystemAssigned = true;
        }

        if (Double.IsNaN(dialog.Height) && !Double.IsNaN(size.Height))
        {
            dialog.Height = size.Height;
            dialog.IsHeightSystemAssigned = true;
        }
    }

    /// <summary>
    /// Calculates the size for a dialog using the given parent and optional limits. Each axis is resolved
    /// independently: an explicitly set <see cref="Layoutable.Width" /> or <see cref="Layoutable.Height" />
    /// always wins; unset axes are derived from the size mode (parent-derived scale for Small/Medium/Large,
    /// content-hug (NaN) for Custom when the other axis is set).
    /// </summary>
    public static Size GetSize(this Dialog dialog, Layoutable? parent, Size? minSize = null, Size? maxSize = null, DialogSize? dialogSize = null)
    {
        var options = Navigation.Dialogs;
        var size = dialogSize ?? dialog.DialogSize;

        var width = dialog.Width;
        var height = dialog.Height;
        var widthUnset = Double.IsNaN(width);
        var heightUnset = Double.IsNaN(height);

        // Fully explicit dialog
        if (!widthUnset && !heightUnset)
            return new Size(width, height);

        // Custom with exactly one unset axis: the unset axis hugs the content (NaN)
        if (size == DialogSize.Custom && widthUnset != heightUnset)
            return new Size(width, height);

        if (widthUnset && heightUnset)
        {
            if (size == DialogSize.Custom)
                Debug.WriteLine($"Warning: Dialog '{dialog.GetType().Name}' uses DialogSize.Custom without Width/Height set - falling back to 0.5x0.5 of the parent.");

            if (parent == null)
                return options.FallbackSize;

            var bothUnsetSpec = GetScaleSpec(size, dialog, options, minSize, maxSize)
                                ?? throw new ArgumentOutOfRangeException(nameof(dialog.DialogSize), size, null);
            return GetSize(GetBaseSize(parent), bothUnsetSpec.Scale, bothUnsetSpec.MinSize, bothUnsetSpec.MaxSize);
        }

        // Exactly one axis unset: derive it from the parent (scale for Small/Medium/Large, fallback size without a parent)
        if (parent == null)
            return new Size(
                widthUnset ? options.FallbackSize.Width : width,
                heightUnset ? options.FallbackSize.Height : height);

        var scaleSpec = GetScaleSpec(size, dialog, options, minSize, maxSize)
                        ?? throw new ArgumentOutOfRangeException(nameof(dialog.DialogSize), size, null);
        var scaledSize = GetSize(GetBaseSize(parent), scaleSpec.Scale, scaleSpec.MinSize, scaleSpec.MaxSize);

        return new Size(
            widthUnset ? scaledSize.Width : width,
            heightUnset ? scaledSize.Height : height);
    }

    #region Private

    private static (Size Scale, Size? MinSize, Size? MaxSize)? GetScaleSpec(DialogSize size, Dialog dialog, DialogOptions options, Size? minSize, Size? maxSize)
    {
        return size switch
        {
            DialogSize.Small => (dialog.SizeScale ?? options.SmallScale, minSize ?? dialog.MinSize ?? options.SmallMinSize, maxSize ?? dialog.MaxSize ?? options.SmallMaxSize),
            DialogSize.Medium => (dialog.SizeScale ?? options.MediumScale, minSize ?? dialog.MinSize ?? options.MediumMinSize, maxSize ?? dialog.MaxSize ?? options.MediumMaxSize),
            DialogSize.Large => (dialog.SizeScale ?? options.LargeScale, minSize ?? dialog.MinSize ?? options.LargeMinSize, maxSize ?? dialog.MaxSize ?? options.LargeMaxSize),
            DialogSize.Custom => (new Size(0.5, 0.5), null, null),
            _ => null
        };
    }

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
