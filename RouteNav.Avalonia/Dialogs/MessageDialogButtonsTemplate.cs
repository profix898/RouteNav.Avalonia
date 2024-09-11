using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace RouteNav.Avalonia.Dialogs;

public sealed class MessageDialogButtonsTemplate : IDataTemplate
{
    #region Implementation of IDataTemplate

    public bool Match(object data)
    {
        return data is MessageDialog;
    }

    public Control Build(object data)
    {
        var messageDialog = data as MessageDialog;
        Debug.Assert(messageDialog != null);

        var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

        if (messageDialog.Buttons is MessageDialogButtons.Ok or MessageDialogButtons.OkCancel)
            AddButton(messageDialog, stackPanel, "Ok", MessageDialogResult.Ok);
        if (messageDialog.Buttons is MessageDialogButtons.YesNo or MessageDialogButtons.YesNoCancel)
        {
            AddButton(messageDialog, stackPanel, "Yes", MessageDialogResult.Yes);
            AddButton(messageDialog, stackPanel, "No", MessageDialogResult.No);
        }
        if (messageDialog.Buttons is MessageDialogButtons.OkCancel or MessageDialogButtons.YesNoCancel)
            AddButton(messageDialog, stackPanel, "Cancel", MessageDialogResult.Cancel);

        return stackPanel;
    }

    #endregion

    private static void AddButton(MessageDialog messageDialog, Panel stackPanel, string caption, MessageDialogResult result)
    {
        var button = new Button { Content = caption };
        button.Classes.Add("MessageDialogButton");
        button.Click += (_, _) => messageDialog.Close(result);

        stackPanel.Children.Add(button);
    }
}
