# Advanced Topics

This page covers RouteNav extension points and patterns that are useful once the basics are in place.

## Custom Page Resolver

Use `IPageResolver` when pages are dynamic or too numerous to register individually.

```csharp
public sealed class MyPageResolver : IPageResolver
{
    public Page? ResolveRoute(Uri routeUri)
    {
        if (routeUri.AbsolutePath.EndsWith("/reports"))
            return new ReportsPage();

        return null;
    }
}
```

Attach it to a stack:

```csharp
mainStack.PageResolver = new MyPageResolver();
```

RouteNav asks `PageResolver` before looking at registered page factories.

## Page Factories

Use page factories when page creation needs route-specific logic:

```csharp
stack.AddPage("details", uri =>
{
    var query = uri.ParseQueryString();
    return new DetailsPage(query["id"]);
});
```

If you construct pages yourself, remember that RouteNav only injects `PageQuery` automatically when pages are resolved through `Navigation.UIPlatform.GetPage(...)`. You can still parse the URI yourself or set properties directly.

## Custom Error View Factory

Replace the error view used by `Error.Page(...)`, `Error.Dialog(...)` and `Error.ShowDialog(...)`:

```csharp
public sealed class MyErrorViewFactory : IErrorViewFactory
{
    public object BuildErrorView(string message, string? exceptionDetails)
    {
        return new ErrorPanel
        {
            Message = message,
            Details = exceptionDetails
        };
    }
}

Error.ErrorFactory = new MyErrorViewFactory();
```

## Custom UI Platform

Most apps use the default `AvaloniaUIPlatform`, but advanced hosts can provide a custom `IUIPlatform`:

```csharp
builder.UseRouteNavUIPlatform("https://myapp.local/", myPlatform);
```

A custom platform controls:

- Page registration and resolution.
- Stack storage and activation.
- Window/dialog creation through `IWindowManager`.
- External URI/file launching through `ILauncher`.

All navigation operations should run on the UI thread. The default `AvaloniaUIPlatform` is not thread-safe and assumes UI-thread serialized access.

## Custom Stacks

Create a custom stack by deriving from `NavigationStackBase<TContainer>`.

```csharp
public sealed class MyStack : NavigationStackBase<MyContainer>
{
    public MyStack(string name, string title)
        : base(name, title)
    {
    }

    protected override MyContainer InitContainer()
    {
        RootPage = new LazyValue<Page>(() => ResolveRoute(this.BuildRoute(String.Empty))
            ?? throw new NavigationException("Root page not found."));

        return new MyContainer { NavigationStack = this };
    }
}
```

Override page/dialog behavior only when you need layout-specific semantics. The base class already handles route resolution, page history, dialog stacks and navigation events.

## RouteEventStack for Action Routes

`RouteEventStack` is a lightweight way to handle route-like commands.

```csharp
var actions = new RouteEventStack("actions", uri =>
{
    if (uri.AbsolutePath.EndsWith("/help"))
        return new HelpPage();

    if (uri.AbsolutePath.EndsWith("/sync"))
    {
        StartSync();
        return null;
    }

    return null;
});

Navigation.UIPlatform.AddStack(actions);
```

If the handler returns a page, it is shown as a dialog on the main stack.

## URI Activation Pattern

A typical platform URI activation handler should:

1. Normalize the platform URI to match `Navigation.BaseRouteUri`.
2. Call `Navigation.PushAsync(uri)`.
3. Choose a `NavigationTarget` if the activation should open a dialog/window.

```csharp
public Task OpenUriAsync(Uri uri)
{
    return Navigation.PushAsync(uri);
}
```

Use a base URI that matches your app's scheme/host:

```csharp
builder.UseRouteNavUIPlatform("myapp://open/", serviceProvider, services);
```

## Testing Navigation

Because navigation is URI-based, tests can drive the app through the same public API as the UI.

```csharp
await Navigation.PushAsync("main", "details?id=42");

var stack = Navigation.GetMainStack();
Assert.IsType<DetailsPage>(stack.CurrentPage);
```

For page-specific assertions, read `CurrentPage`, `PageStack`, `DialogStack` and the navigation events.

## Common Pitfalls

### Missing Root Page

Every normal stack should have a root page registered with an empty route:

```csharp
stack.AddPage<HomePage>(String.Empty);
```

### Wrong Base URI

If absolute routes do not resolve, check that they are based on `Navigation.BaseRouteUri`.

### Factory Pages and PageQuery

If you use a factory that directly creates a page, RouteNav does not automatically set `PageQuery` through DI. Parse the URI in the factory or use `Navigation.UIPlatform.GetPage(...)`.

### Cross-Stack Links

Use absolute route paths (`/stack/page`) when linking between stacks from XAML or sidebar menu items.

### Dialogs on Single-View Platforms

Mobile/browser heads use overlays, not native dialog windows. Design dialog content to work as an in-app overlay and avoid desktop-only assumptions.
