using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Reactive;

namespace RouteNav.Avalonia.Dialogs;

public class OverlayHost : ContentControl
{
    private IDisposable? boundsWatcher;

    public OverlayHost()
    {
        Background = null;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
    }

    protected override Type StyleKeyOverride => typeof(OverlayPopupHost);

    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);

        return VisualRoot switch
        {
            TopLevel topLevel => topLevel.ClientSize,
            Control control => control.Bounds.Size,
            _ => default(Size)
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        if (e.Root is Control control)
        {
            var observer = new AnonymousObserver<Rect>(_ => InvalidateMeasure());
            boundsWatcher = control.GetObservable(BoundsProperty).Subscribe(observer);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        boundsWatcher?.Dispose();
        boundsWatcher = null;
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        e.Handled = true;
    }
}
