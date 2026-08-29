# Concepts

RouteNav.Avalonia is built around a few core concepts: routes, pages, stacks, containers, targets and the platform window abstraction.

## Route-First Navigation

RouteNav treats navigation as an addressable operation. Instead of a button directly replacing a view, the button (or command, menu item, URI activation handler, etc.) asks the central `Navigation` facade to open a URI.

```csharp
await Navigation.PushAsync(new Uri("https://app.local/main/orders/42"));
```

The navigation system then finds the target stack, resolves the page, updates the appropriate container, and raises navigation events.

This makes navigation independent of the control that triggered it. The same route can be opened from:

- A `RouteButton` in XAML.
- A menu item.
- A command in a view model.
- A URI activation event from the platform.
- A test or automation flow.

## Base Route URI

All absolute internal routes are resolved against `Navigation.BaseRouteUri`. The default is:

```text
https://avalonia.local/
```

A route such as `/main/details` becomes:

```text
https://avalonia.local/main/details
```

You can set the base URI during bootstrapping:

```csharp
builder.UseRouteNavUIPlatform("https://myapp.local/", serviceProvider, services);
```

The base URI is especially useful if your application supports platform URI activation or deep links.

## Pages

A RouteNav page derives from `RouteNav.Avalonia.Page`.

A page is a `ContentControl` with navigation-specific metadata:

- `Title` - used by navigation bars, tab headers and dialog titles.
- `PageQuery` - parsed query parameters from the route URI.
- `DialogSizeHint` - default size hint when the page is shown as a dialog.
- `SafeAreaPadding` - platform safe-area padding propagated from the container.

RouteNav intentionally keeps `Page` separate from Avalonia 12's native `Avalonia.Controls.Page` to avoid conflicts with RouteNav's static `Navigation` facade and to preserve RouteNav's existing API model.

## Navigation Stacks

A navigation stack maps routes to pages and manages the current page/dialog state.

Common stacks:

- `ContentPageStack` - single-page content replacement.
- `NavigationPageStack` - history stack with navigation bar and back button.
- `TabbedPageStack` - tabbed layout populated from registered pages.
- `SidebarMenuPageStack` - drawer/sidebar layout populated from menu items.
- `RouteEventStack` - route event handler for custom URI handling.

Each stack has:

- `Name` - the stack segment in absolute routes, e.g. `main` in `/main/details`.
- `Title` - used for window titles and display contexts.
- `BaseUri` - the base URI for routes on that stack.
- `RootPage` - the page registered at the empty route.
- `PageStack` - current page history (where supported).
- `DialogStack` - active dialogs.

## Containers

A stack does not directly own a window. Instead, each stack has a container control that renders its current page.

Examples:

- `NavigationContainer` hosts content directly.
- `NavigationPageContainer` hosts a `NavigationControl`.
- `TabbedPageContainer` hosts a `TabControl`.
- `SidebarMenuPageContainer` hosts a `SidebarMenu`.

Containers also propagate safe-area padding and handle overlay dialogs.

## Navigation Targets

`NavigationTarget` tells RouteNav where a route should be displayed.

```csharp
public enum NavigationTarget
{
    Self,
    Parent,
    Dialog,
    DialogOverlay,
    Window
}
```

Target behavior:

- `Self` opens in the current navigation context.
- `Parent` opens in the parent/window context.
- `Dialog` opens as a dialog associated with the relevant stack.
- `DialogOverlay` forces overlay dialog display.
- `Window` opens in a new window where supported, otherwise falls back to stack replacement.

Platform support can affect the result. On mobile/browser, new windows are not available, so `Window` falls back to replacing the active stack. Dialogs are shown as overlays when native dialog windows are not available or when `ForceOverlayDialogs` is enabled.

## Window Abstraction

RouteNav provides a `RouteNav.Avalonia.Window` abstraction so the same navigation code works across desktop, mobile and browser.

Set it during application startup:

```csharp
ApplicationLifetime.SetMainWindow(context => new MainWindow());
```

The factory creates a fresh RouteNav window for each platform window/view. RouteNav hosts the returned instance directly; it does not copy appearance properties. This keeps its XAML resources, styles and bindings attached to the normal Avalonia tree. Set `INavigationStack.WindowFactory` when a stack needs a different window type.

## Dialogs

RouteNav dialogs can be shown in several ways:

- A `Page` can be converted to a `Dialog` automatically when opened with `NavigationTarget.Dialog` or `NavigationTarget.DialogOverlay`.
- A `Page` can be converted manually with `page.ToDialog(parent)`.
- A `Dialog` can be shown directly with `ShowDialog(...)`.
- `MessageDialog` provides a simple message-box style dialog.
- The static `Error` factory creates error pages/dialogs.

Desktop platforms can show native dialog windows. Single-view platforms use overlays.

## Page Resolution

A stack resolves a route in this order:

1. Ask `PageResolver` if one is configured.
2. Look up the route in registered page factories.
3. If resolution fails during top-level navigation, RouteNav displays `NotFoundPage` or an error page depending on the failure mode.

Pages resolved through `Navigation.UIPlatform.GetPage(...)` receive `PageQuery` and `routeUri` automatically.

## Events

Stacks expose events for navigation changes:

- `PageNavigated`
- `DialogNavigated`
- `RouteNavigated`
- `Entered`
- `Exited`

These are useful for logging, telemetry, custom back handling and integration tests.
