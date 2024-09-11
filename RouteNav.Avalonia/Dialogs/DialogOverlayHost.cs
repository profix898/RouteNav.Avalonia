using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace RouteNav.Avalonia.Dialogs;

public sealed class DialogOverlayHost : ContentControl, ICustomKeyboardNavigation, IDisposable
{
    private readonly HashSet<Dialog> dialogCollection = new HashSet<Dialog>();
    private readonly TopLevel topLevel;
    private readonly OverlayLayer overlayLayer;
    
    private IInputElement? lastFocusElement;
    private IDisposable? disposable;

    public DialogOverlayHost(TopLevel topLevel, OverlayLayer overlayLayer)
    {
        lastFocusElement = topLevel.FocusManager?.GetFocusedElement();
        
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        this.topLevel = topLevel;
        this.overlayLayer = overlayLayer;
        this.overlayLayer.Children.Add(this);
    }

    protected override Type StyleKeyOverride => typeof(DialogOverlayHost);
    
    public void SetContent(Dialog dialog)
    {
        disposable = dialog.SetSizeBinding(topLevel);
        
        Content = dialog;
        overlayLayer.UpdateLayout();
        
        // Track dialog lifetime
        dialogCollection.Add(dialog);
        dialog.Closed += (_, _) =>
        {
            dialogCollection.Remove(dialog);
            
            // If last dialog gets closed, dispose overlay
            if (dialogCollection.Count == 0)
                Dispatcher.UIThread.Invoke(Dispose);
        };
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);

        return VisualRoot switch
        {
            TopLevel topLevel => topLevel.ClientSize,
            Control control => control.Bounds.Size,
            _ => size
        };
    }
    
    #region Implementation of ICustomKeyboardNavigation

    public (bool handled, IInputElement? next) GetNext(IInputElement element, NavigationDirection direction)
    {
        return element.Equals(this) 
            ? (true, this.GetVisualDescendants().OfType<IInputElement>().FirstOrDefault(visual => visual.Focusable))
            : (false, null);
    }

    #endregion

    #region Implementation of IDisposable

    public void Dispose()
    {
        Content = null;
        disposable?.Dispose();
        overlayLayer.Children.Remove(this);

        if (lastFocusElement != null)
        {
            lastFocusElement.Focus();
            lastFocusElement = null;
        }
    }

    #endregion
}
