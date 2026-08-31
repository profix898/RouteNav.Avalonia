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

`SetMainWindow` replaces the built-in default `WindowFactory` (which supplies a plain RouteNav `Window` shell). Each call must create a fresh, unattached RouteNav window; Android activity factories may invoke it more than once. A navigation stack can override the default for its own windows through `INavigationStack.WindowFactory`; dialog windows always use the application-wide default so custom stack chrome does not wrap dialog content. RouteNav hosts these windows directly rather than cloning properties or bindings.

Declarative OS-window chrome declared on the window shell (`Width`/`Height`, min/max sizes, `CanResize`, `CanMinimize`, `CanMaximize`, `ShowInTaskbar`, `ShowActivated`, `Topmost`, `WindowState`, `WindowStartupLocation`, `WindowDecorations`, `ExtendClientAreaToDecorationsHint`, `TransparencyLevelHint`, `TransparencyBackgroundFallback`) is mirrored onto the platform window. The `WindowCustomizationEvent` remains the escape hatch for everything else.

Shells with custom application chrome (toolbar, menu) around the navigation surface override `SetContentCore` and route the stack content into a dedicated content host — see the demo app's `SidebarWindow` for an example.

## Desktop

Desktop platforms support native windows and dialog windows.

`NavigationTarget.Window` opens a new RouteNav window where possible:

```csharp
await Navigation.PushAsync(uri, NavigationTarget.Window);
```

`NavigationTarget.Dialog` opens a native dialog window unless overlay dialogs are forced.

When a navigation targets a stack hosted in a different window, RouteNav brings that window to the foreground (`Navigation.Windows.BringTargetWindowToFront`, default `true`).

## Stack/Window Introspection

The platform tracks which navigation stack is hosted in which window. `IUIPlatform.ActiveStacks` lists the stacks currently hosted in an open window (in hosting order); combine it with `GetActiveWindowFromStack(stack)` and `GetActiveStackFromWindow(window)` to map between stacks and windows:

```csharp
foreach (var stack in Navigation.UIPlatform.ActiveStacks)
{
    var window = Navigation.UIPlatform.GetActiveWindowFromStack(stack);
    // e.g. for diagnostics, window-level UI or taskbar customization
}
```

## Window Lifecycle

Closing the main window unhosts the main navigation stack while preserving its state (current page and history). Navigating to a route of the unhosted main stack re-opens the main window (a fresh shell is created through the window factory) and shows the pushed route. When a stack switch re-opens the main window (e.g. popping the last page of a secondary stack), the vacated source window is closed. Closing a secondary window resets its stack; navigating to it again activates it in the current window.

`RouteNav.Avalonia.Window` exposes `IsClosed`, and `Dialog` exposes `IsClosed` for lifecycle checks.

Apps that want different close semantics can opt in themselves: `ShutdownMode.OnMainWindowClose` shuts the application down when the main window closes, and the window's cancelable `Closing` event can prevent closing (or hide the window instead) — e.g. while secondary windows are open.

Global options live directly on the `Navigation` facade: `Navigation.MainStackName` (default `main`; set at startup before stacks are registered), `Navigation.BaseRouteUri`, `Navigation.Windows` (`WindowOptions`) and `Navigation.Dialogs` (`DialogOptions`).

Force single-window mode:

```csharp
Navigation.Windows.ForceSingleWindow = true;
```

Force overlay dialogs:

```csharp
Navigation.Windows.ForceOverlayDialogs = true;
```

Global options are grouped on the `Navigation` facade: `Navigation.MainStackName` and `Navigation.BaseRouteUri` for routing, `Navigation.Windows` (`WindowOptions`: `ForceSingleWindow`, `ForceOverlayDialogs`, `BringTargetWindowToFront`) and `Navigation.Dialogs` (`DialogOptions`: sizing defaults, overlay close animation).

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

Before the event fires, RouteNav mirrors declarative chrome from the window shell onto the platform window (sizes, `CanResize`, transparency hints, decorations, state — see above). The built-in manager then exposes `WindowCustomizationEvent` for created platform windows:

```csharp
windowManager.WindowCustomizationEvent += (window, isDialog) =>
{
    window.CanResize = !isDialog;
};
```

This is useful for platform-specific styling, owner behavior or window flags not covered by the shell mirror.

## Dev Tools

Avalonia 12 uses `AvaloniaUI.DiagnosticsSupport` and application-level developer tools:

```csharp
#if DEBUG
this.AttachDeveloperTools();
#endif
```

RouteNav no longer attaches dev tools per created window.
