# Routing

RouteNav.Avalonia routes are URIs. A route identifies a stack and, within that stack, a page.

## Route Shape

A typical route looks like this:

```text
/main/details?id=42
```

With the default base URI it resolves to:

```text
https://avalonia.local/main/details?id=42
```

Route parts:

- `main` - stack name.
- `details` - route path on the stack.
- `?id=42` - query string parsed into `Page.PageQuery`.

## Absolute vs Relative Routes

Absolute route paths start with `/` and include the target stack:

```text
/main/details
/sidebar/settings
/tabbed/page1
```

Relative routes do not start with `/` and are resolved against the current stack:

```text
details
settings
```

Routing controls expose `RoutePath` for both styles:

```xml
<routing:RouteButton RoutePath="/main/details" Content="Absolute route" />
<routing:RouteButton RoutePath="details" Content="Relative route" />
```

## Building Routes in Code

Use `Navigation.BuildRoute(...)` when you have a stack name:

```csharp
var uri = Navigation.BuildRoute("main", "details?id=42");
await Navigation.PushAsync(uri);
```

Use stack extension methods when you already have a stack:

```csharp
var uri = mainStack.BuildRoute("details?id=42");
await mainStack.PushAsync(uri);
```

## Navigating

The central facade supports several forms:

```csharp
await Navigation.PushAsync(new Uri("https://avalonia.local/main/details"));
await Navigation.PushAsync("main", "details");
await Navigation.PushAsync(uri, NavigationTarget.Dialog);
```

Stacks also support direct navigation:

```csharp
await mainStack.PushAsync("details");
await mainStack.PushAsync(new Uri("details", UriKind.Relative));
```

## Query Parameters

Query parameters are parsed into `Page.PageQuery`.

```csharp
await Navigation.PushAsync(new Uri("https://avalonia.local/main/details?id=42&mode=edit"));
```

Read them in the target page:

```csharp
if (PageQuery.TryGetValue("id", out var id))
{
    // use id
}
```

RouteNav also adds:

```text
routeUri
```

This contains the full route URI that created the page. It is useful for stack identification, diagnostics and dialog-parent lookup.

## Query Helpers

Use `AddQueryString` to add parameters to a URI:

```csharp
var uri = Navigation
    .BuildRoute("main", "details")
    .AddQueryString("id", "42");
```

Or add several parameters:

```csharp
var uri = Navigation.BuildRoute("main", "details")
    .AddQueryString(new Dictionary<string, string>
    {
        ["id"] = "42",
        ["mode"] = "edit"
    });
```

## Navigation Targets

`NavigationTarget` controls where the resolved page is shown.

### Self

Opens in the current navigation context. If the route targets another stack, RouteNav may activate or request that stack depending on the current container.

```csharp
await Navigation.PushAsync(uri, NavigationTarget.Self);
```

### Parent

Opens in the parent/window context, replacing the current stack/container where appropriate.

```csharp
await Navigation.PushAsync(uri, NavigationTarget.Parent);
```

### Dialog

Resolves the route to a page and shows it as a dialog.

```csharp
await Navigation.PushAsync(uri, NavigationTarget.Dialog);
```

On desktop, this prefers a dialog window. On single-view platforms, or when overlay dialogs are forced, it uses an overlay.

### DialogOverlay

Forces overlay display even on desktop.

```csharp
await Navigation.PushAsync(uri, NavigationTarget.DialogOverlay);
```

### Window

Opens the target stack in a new window where supported.

```csharp
await Navigation.PushAsync(uri, NavigationTarget.Window);
```

On mobile/browser, this falls back to stack replacement because those platforms do not support multiple windows.

## Route Resolution

When you navigate to a route, RouteNav:

1. Determines the stack name from the absolute route URI.
2. Finds or activates the target stack.
3. Checks whether the route is based on that stack's `BaseUri`.
4. Resolves the route through `PageResolver` or registered page factories.
5. Injects query parameters into `Page.PageQuery`.
6. Updates the page/dialog/window target.

If the stack cannot resolve the page, RouteNav shows `NotFoundPage` in the active context.

## Cross-Stack Navigation

Absolute routes can move between stacks:

```csharp
await Navigation.PushAsync(new Uri("https://avalonia.local/sidebar/settings"));
await Navigation.PushAsync(new Uri("https://avalonia.local/tabbed/page1"), NavigationTarget.Window);
```

A sidebar menu item can link to another stack:

```csharp
sidebarStack.AddMenuItem("/tabbed/page1", "Tabbed: Summary");
```

## RouteEventStack

`RouteEventStack` handles route events without maintaining page history. It is useful for plugin-like or command-like routes.

```csharp
var eventStack = new RouteEventStack("actions", uri =>
{
    if (uri.AbsolutePath.EndsWith("/about"))
        return new AboutPage();

    return null;
});

Navigation.UIPlatform.AddStack(eventStack);
```

If the event handler returns a page, RouteNav shows it as a dialog on the main stack.

## URI Activation

For platform URI activation, convert the platform activation URI to a RouteNav URI and call:

```csharp
await Navigation.PushAsync(activationUri);
```

If the activation URI uses your app's scheme/host, set `Navigation.BaseRouteUri` (or use `UseRouteNavUIPlatform`) to match that URI scheme.
