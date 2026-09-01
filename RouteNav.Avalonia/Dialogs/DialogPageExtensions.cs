using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Dialogs;

/// <summary>Extensions for showing a <see cref="Page" /> as a <see cref="Dialog" />.</summary>
public static class DialogPageExtensions
{
    /// <summary>Wraps the page in a dialog and pushes it onto the stack's dialog stack.</summary>
    public static Task<object?> PushDialogAsync(this INavigationStack stack, Page page, DialogSize? dialogSize = null,
                                                Size? minSize = null, Size? maxSize = null, bool forceOverlay = false)
    {
        return stack.PushDialogAsync(page.ToDialog(stack.CurrentPage, dialogSize, minSize, maxSize), forceOverlay);
    }

    /// <summary>Builds a <see cref="Dialog" /> that hosts the given page, sized from the page's explicit size axes and the size hint/parent.</summary>
    public static Dialog ToDialog(this Page page, Layoutable? parent = null, DialogSize? dialogSize = null,
                                  Size? minSize = null, Size? maxSize = null)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        dialogSize ??= page.DialogSizeHint ?? DialogSize.Medium;

        // Build Dialog from Page
        var dialog = new Dialog
        {
            Title = page.Title ?? "Dialog", Content = page, DataContext = page.DataContext, Background = page.Background ?? Brushes.White, DialogSize = dialogSize.Value,
            SizeScale = page.SizeScaleHint, MinSize = page.MinSizeHint, MaxSize = page.MaxSizeHint
        };

        // Transfer the page's explicit size axes (they win over derived sizes), then derive the remaining axes from the parent
        dialog.Width = page.Width;
        dialog.Height = page.Height;
        dialog.SetSize(parent, minSize, maxSize);

        return dialog;
    }
}
