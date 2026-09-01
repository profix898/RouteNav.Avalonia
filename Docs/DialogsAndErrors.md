# Dialogs and Errors

RouteNav.Avalonia supports dialogs as first-class navigation targets. A page can be shown as a dialog, dialogs can be shown directly, and error pages/dialogs are created through a shared factory.

## Dialog Targets

You can navigate to any registered page as a dialog:

```csharp
await Navigation.PushAsync(
    new Uri("https://avalonia.local/main/details?id=42"),
    NavigationTarget.Dialog);
```

Use `DialogOverlay` to force overlay display:

```csharp
await Navigation.PushAsync(uri, NavigationTarget.DialogOverlay);
```

Desktop platforms prefer native dialog windows for `Dialog`; single-view platforms (mobile/browser) and forced-overlay mode use in-app overlays.

## Page to Dialog Conversion

A `Page` is converted to a `Dialog` with `ToDialog(...)`.

```csharp
var page = new DetailsPage
{
    Title = "Details",
    DialogSizeHint = DialogSize.Medium
};

var dialog = page.ToDialog(parent);
var result = await dialog.ShowDialog(this);
```

When a page is opened through `NavigationTarget.Dialog`, RouteNav performs this conversion automatically.

The dialog title is taken from `Page.Title`. The initial size is controlled by `Page.DialogSizeHint` unless you pass an explicit size hint.

## Dialog Sizing

A dialog's size is resolved per axis (width and height independently), and the same rule applies in every display mode (dialog window, overlay, embedded):

| Axis state | Resulting size |
|---|---|
| `Width` / `Height` explicitly set on the dialog (or page) | Used as-is |
| Axis unset, `DialogSize` = Small / Medium / Large | Derived from the parent with the size scale, clamped to min/max |
| Axis unset, `DialogSize` = `Custom`, other axis set | Content-hugging: the axis sizes to the dialog content |
| Both axes unset, `DialogSize` = `Custom` | Falls back to 0.5×0.5 of the parent (a warning is written to the debug output) |

Scale-derived axes track the parent when it resizes. Explicitly set and content-hugging axes are never overwritten by the size system.

Setting one axis is enough — a fixed width with an auto-adjusting height is a single property:

```csharp
var dialog = new Dialog
{
    Title = "Progress",
    DialogSize = DialogSize.Custom,
    Width = 520 // height hugs the content
};
```

`DialogSize.Custom` only changes the behavior of *unset* axes: they hug the content instead of scaling with the parent. `Small`, `Medium` and `Large` scale both unset axes.

### Scale, Min and Max

Scale-derived axes use the scale and min/max limits of the active `DialogSize`. These defaults are configured through `Navigation.Dialogs` (`DialogOptions`):

```csharp
Navigation.Dialogs.SmallScale = new Size(0.3, 0.3);
Navigation.Dialogs.MediumScale = new Size(0.5, 0.5);
Navigation.Dialogs.LargeScale = new Size(0.8, 0.8);
Navigation.Dialogs.FallbackSize = new Size(400, 300);
```

A dialog can override the scale and limits per instance (`Dialog.SizeScale`, `Dialog.MinSize`, `Dialog.MaxSize`), and a page can carry the same values as hints (`Page.SizeScaleHint`, `Page.MinSizeHint`, `Page.MaxSizeHint`), which are transferred to the dialog when the page is converted:

```csharp
var page = new DetailsPage
{
    DialogSizeHint = DialogSize.Custom,
    Width = 520,          // fixed width, height hugs the content
    MinSizeHint = new Size(400, 300)
};
```

Precedence for scale-derived axes: explicitly passed `minSize`/`maxSize` (e.g. `stack.PushDialogAsync(page, minSize, maxSize)`) > dialog/page instance values > global `DialogOptions`. Limits apply to scale-derived axes only — explicit and content-hugging axes are never clamped.

### Content Mode

`Dialog.DialogContentMode` controls how the dialog body hosts content:

- `Scroll` — the body is hosted in a scroll viewer whose height is constrained to the dialog frame, so oversized content scrolls inside the frame.
- `Wrap` — the body is hosted unconstrained and the frame grows and shrinks with the content. Content that exceeds an explicitly set height is clipped.

`Scroll` is the default and fits dialogs with a bounded frame; `Wrap` fits dialogs that should always exactly contain their content (e.g. progress dialogs with a fixed width and wrapping text).

### Display Modes

- **Dialog window** (desktop): explicit axes size the OS window; content-hugging axes size the window to its content.
- **Overlay**: scale-derived axes bind to the host; explicit axes are kept; content-hugging axes size the frame to its content.
- **Embedded** (`ShowDialogEmbedded`): only scale-derived axes bind to the host control; explicit and content-hugging axes are left untouched.

## Showing Dialogs Directly

Use `Dialog.ShowDialog(...)` when you already have a dialog instance.

```csharp
var dialog = new Dialog
{
    Title = "Custom",
    Content = new MyDialogContent(),
    DialogSize = DialogSize.Medium
};

var result = await dialog.ShowDialog(this);
```

Overloads:

```csharp
Task<object?> ShowDialog(Window? parentWindow = null, bool forceOverlay = false)
Task<object?> ShowDialog(Page? parentPage, bool forceOverlay = false)
```

When a dialog closes through `Close(object? result)`, that value becomes `Dialog.Result` and completes the `ShowDialog` task.

## Embedded Dialogs

A dialog can be embedded into any `ContentControl`:

```csharp
await dialog.ShowDialogEmbedded(hostContentControl, restoreParent: true);
```

If `restoreParent` is true, the original content is restored when the dialog closes.

## MessageDialog

`MessageDialog` is a simple message-box style dialog. By default it uses a fixed width (400) with a content-hugging height, so the frame exactly fits its content; set `Width`/`Height`/`DialogSize` on the instance to change that.

```csharp
var result = await MessageDialog
    .Create("Delete item", "Are you sure?", MessageDialogButtons.YesNoCancel)
    .ShowDialog(this);

if (result is MessageDialogResult.Yes)
{
    // delete
}
```

Button sets:

- `Ok`
- `OkCancel`
- `YesNo`
- `YesNoCancel`

Results:

- `Ok`
- `Cancel`
- `Yes`
- `No`
- `None`

You can provide custom content:

```csharp
var dialog = MessageDialog.Create("Title", new MyContentControl(), MessageDialogButtons.OkCancel);
```

## Error Pages and Dialogs

The `Error` factory creates error pages and dialogs:

```csharp
Page page = Error.Page("Failed to load data", exception);
Dialog dialog = Error.Dialog("Failed to save", exception);
await Error.ShowDialog("Failed to save", exception, this);
```

RouteNav uses this factory for internal error and not-found scenarios.

## Custom Error Views

Replace `Error.ErrorFactory` to customize the visual used in error pages/dialogs:

```csharp
public sealed class MyErrorViewFactory : IErrorViewFactory
{
    public object BuildErrorView(string message, string? exceptionDetails)
    {
        return new MyErrorView
        {
            Message = message,
            Details = exceptionDetails
        };
    }
}

Error.ErrorFactory = new MyErrorViewFactory();
```

## Overlay Behavior

Overlay dialogs are hosted by `DialogOverlayHost` in the current `TopLevel` overlay layer. RouteNav uses overlays when:

- The platform does not support multiple windows.
- `Navigation.Windows.ForceOverlayDialogs` is true.
- The navigation target is `DialogOverlay`.
- A dialog is already being shown as an overlay and needs to be updated.

## Desktop Behavior

On desktop, `NavigationTarget.Dialog` opens a native dialog window unless forced to overlay. The dialog window is non-resizable by default and inherits title/icon information from the parent RouteNav window where available.
