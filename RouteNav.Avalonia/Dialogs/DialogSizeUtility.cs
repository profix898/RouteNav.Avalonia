using System;
using Avalonia;
using static System.Math;

namespace RouteNav.Avalonia.Dialogs;

public enum DialogSize
{
    Small,
    Medium,
    Large
}

public static class DialogSizeUtility
{
    #region Default

    public static Size GetSize(this Page parentPage, DialogSize dialogSize, Size? minSize = null, Size? maxSize = null)
    {
        if (parentPage == null)
            throw new ArgumentNullException(nameof(parentPage));

        return dialogSize switch
        {
            DialogSize.Small => GetSize(parentPage, new Size(0.3, 0.3), minSize ?? new Size(200, 200), maxSize ?? new Size(400, 400)),
            DialogSize.Medium => GetSize(parentPage, new Size(0.6, 0.6), minSize ?? new Size(350, 350), maxSize ?? new Size(700, 700)),
            DialogSize.Large => GetSize(parentPage, new Size(0.9, 0.9), minSize ?? new Size(500, 500), maxSize ?? new Size(1000, 1000)),
            _ => throw new ArgumentOutOfRangeException(nameof(dialogSize), dialogSize, null)
        };
    }

    public static Size GetSize(this Page parentPage, Size scale, Size? minSize = null, Size? maxSize = null)
    {
        if (parentPage == null)
            throw new ArgumentNullException(nameof(parentPage));

        var baseSize = new Size(parentPage.Width, parentPage.Height);

        return GetSize(baseSize, scale, minSize, maxSize);
    }

    #endregion

    #region SizeCalculation

    public static Size GetSize(Size baseSize, Size scale, Size? minSize = null, Size? maxSize = null)
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
