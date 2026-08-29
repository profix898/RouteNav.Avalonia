# Platform Notes

RouteNav.Avalonia abstracts over Avalonia's desktop, mobile and browser application models. This page explains how the `Window` abstraction and platform manager behave.

## RouteNav Window vs Avalonia Window

RouteNav's `Window` is a platform-neutral abstraction. It may be hosted by:

- An Avalonia `Window` on desktop.
- A single-view `Control` on iOS/browser.
- An activity-created view on Android.

Set the main RouteNav window during application startup:

```csharp
ApplicationLifetime.SetMainWindow(context => new MainWindow());
```

RouteNav tracks the main window independently of the Avalonia lifetime so `Application.Current.GetMainWindow()` and dialog/window ownership work across desktop, single-view and activity lifetimes.

`SetMainWindow` installs the application-wide default `WindowFactory`. Each call must create a fresh, unattached RouteNav window; Android activity factories may invoke it more than once. A navigation stack can override the default through `INavigationStack.WindowFactory`. RouteNav hosts these windows directly rather than cloning properties or bindings.

## Desktop

Desktop platforms support native windows and dialog windows.

`NavigationTarget.Window` opens a new RouteNav window where possible:

```csharp
await Navigation.PushAsync(uri, NavigationTarget.Window);
```

`NavigationTarget.Dialog` opens a native dialog window unless overlay dialogs are forced.

Force single-window mode:

```csharp
Navigation.UIPlatform.WindowManager.ForceSingleWindow = true;
```

Force overlay dialogs:

```csharp
Navigation.UIPlatform.WindowManager.ForceOverlayDialogs = true;
```

These flags are init-only on the built-in `AvaloniaWindowManager`, so set them when constructing a custom manager/platform.

## Mobile and Browser

Mobile/browser heads do not support RouteNav multi-window behavior. In those environments:

- `NavigationTarget.Window` falls back to replacing the current stack/view.
- Dialogs are shown as overlays.
- `ApplicationLifetime.SetMainWindow(...)` creates/assigns the platform view.

RouteNav's navigation API stays the same across heads.

## Android (Avalonia 12)

Avalonia 12 Android uses `IActivityApplicationLifetime` and `MainViewFactory`. RouteNav's `SetMainWindow(...)` overload detects that lifetime and registers the platform view factory.

A typical Android head uses non-generic `AvaloniaMainActivity` plus an `AvaloniaAndroidApplication<App>` class:

```csharp
[Activity(Label = "MyApp.Android", MainLauncher = true)]
public class MainActivity : AvaloniaMainActivity { }

[Application]
public class AndroidApp : AvaloniaAndroidApplication<App>
{
    protected AndroidApp(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer) { }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder)
            .UseRouteNavUIPlatform("https://myapp.local/", serviceProvider, services);
}
```

Do not use Avalonia 11's old generic `AvaloniaMainActivity<TApp>` pattern.

## iOS

The standard `AvaloniaAppDelegate<App>` pattern is used. RouteNav runs through the single-view lifetime, so dialogs are overlays and window navigation falls back to stack replacement.

## Browser

Browser heads use `Avalonia.Browser`. RouteNav runs through the single-view lifetime. New windows are not created by RouteNav; use external URI launching for browser tabs/windows where appropriate.

## Custom IWindowManager

Implement `IWindowManager` when you need custom platform window creation or dialog behavior.

Key members:

```csharp
bool SupportsMultiWindow { get; }
bool ForceSingleWindow { get; init; }
bool ForceOverlayDialogs { get; init; }

bool OpenWindow(Window window, Window? parentWindow = null);
bool OpenDialog(Dialog dialog, out Task<object?> dialogTask, Window? parentWindow = null);

Avalonia.Controls.Window CreatePlatformWindow(
    Window window,
    IClassicDesktopStyleApplicationLifetime desktopLifetime,
    bool isDialogWindow = false);

ContentControl CreatePlatformView(
    Window window,
    IApplicationLifetime appLifetime);
```

Avalonia 12 note: `CreatePlatformView` accepts `IApplicationLifetime`, not only `ISingleViewApplicationLifetime`, so it can serve Android activity lifetimes too.

## Window Customization

The built-in manager exposes `WindowCustomizationEvent` for created platform windows:

```csharp
windowManager.WindowCustomizationEvent += (window, isDialog) =>
{
    window.CanResize = !isDialog;
};
```

This is useful for platform-specific styling, owner behavior or window flags.

## Dev Tools

Avalonia 12 uses `AvaloniaUI.DiagnosticsSupport` and application-level developer tools:

```csharp
#if DEBUG
this.AttachDeveloperTools();
#endif
```

RouteNav no longer attaches dev tools per created window.
