using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Dialogs;

public static class DialogPageExtensions
{
    public static Task PushDialogAsync(this INavigationStack stack, Page page, DialogSize dialogSize = DialogSize.Medium,
                                       Size? minSize = null, Size? maxSize = null)
    {
        return stack.PushDialogAsync(page.ToDialog(stack.CurrentPage, dialogSize, minSize, maxSize));
    }

    public static Dialog ToDialog(this Page page, Page parentPage, DialogSize dialogSize = DialogSize.Medium,
                                  Size? minSize = null, Size? maxSize = null)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        // Build Dialog from Page
        return new Dialog
        {
            Title = page.Title ?? "Dialog",
            Content = page.Content,
            DataContext = page.DataContext,
            Resources = page.Resources,
            Background = page.Background ?? Brushes.LightGray,
            Size = parentPage.GetSize(dialogSize, minSize, maxSize)
        };
    }
}
