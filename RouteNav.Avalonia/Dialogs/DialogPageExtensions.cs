using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using RouteNav.Avalonia.Stacks;
using static System.Double;
using static RouteNav.Avalonia.Dialogs.DialogSizeUtility;

namespace RouteNav.Avalonia.Dialogs;

public static class DialogPageExtensions
{
    public static Task<object?> PushDialogAsync(this INavigationStack stack, Page page, DialogSize? dialogSize = null,
                                                Size? minSize = null, Size? maxSize = null, bool forceOverlay = false)
    {
        return stack.PushDialogAsync(page.ToDialog(stack.CurrentPage, dialogSize, minSize, maxSize), forceOverlay);
    }

    public static Dialog ToDialog(this Page page, Layoutable? parent = null, DialogSize? dialogSize = null,
                                  Size? minSize = null, Size? maxSize = null)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        dialogSize ??= page.DialogSizeHint ?? DialogSize.Medium;
        var size = GetSize(dialogSize.Value, parent, minSize, maxSize);

        // Special case: custom size
        if (dialogSize == DialogSize.Custom)
        {
            if (!IsNaN(page.Width) && !IsNaN(page.Height)) // Custom size from page
                size = new Size(page.Width, page.Height);
            else if (parent != null && !IsNaN(parent.Width) && !IsNaN(parent.Height)) // Custom size from parent
                size = new Size(parent.Width, parent.Height);
        }
        
        // Build Dialog from Page
        var dialog = new Dialog
        {
            Title = page.Title ?? "Dialog",
            Content = page,
            DataContext = page.DataContext,
            Background = page.Background ?? Brushes.White,
            DialogSize = dialogSize.Value
        };

        // Adapt page size via dialog size
        page.Bind(Layoutable.WidthProperty, dialog.GetBindingObservable(Layoutable.WidthProperty, width => IsNaN(width) ? size.Width : width));
        page.Bind(Layoutable.HeightProperty, dialog.GetBindingObservable(Layoutable.HeightProperty, height => IsNaN(height) ? size.Height : height));
        dialog.Width = size.Width;
        dialog.Height = size.Height;

        return dialog;
    }
}
