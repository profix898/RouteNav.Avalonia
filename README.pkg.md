# RouteNav.Avalonia

**RouteNav.Avalonia** provides URI-based navigation for **[Avalonia](https://avaloniaui.net/)**. It supports a code-first, modular/extensible navigation model built around page, dialog and stack primitives.

## Features

- **Central URI navigation** via `Navigation.PushAsync(uri, target)` and route-aware controls.
- **Four navigation layouts**: single page, navigation stack with back button, tabs, and sidebar/drawer navigation built on Avalonia 12's native `DrawerPage`.
- **Dialogs and overlays**: show pages as dialogs, use built-in message dialogs, and display user-facing error pages or dialogs.
- **Cross-stack navigation targets**: route to `Self`, `Parent`, `Dialog`, `DialogOverlay` or `Window`.
- **Dependency-injection friendly page registration**, with custom page factories and route resolvers when routes are dynamic.
- **Desktop, mobile and browser support** through one RouteNav `Window` abstraction, including multi-window desktop and single-view mobile/browser behavior.
- **Routing controls**: `RouteButton`, `RouteMenuItem`, `RouteCommand`, `HyperlinkButton` and `HyperlinkLabel`.

## Quick start

Register pages during application startup:

```csharp
Navigation.UIPlatform.RegisterPage<RootPage, Page1>();
```

Create a stack and map routes to pages:

```csharp
var stack = new NavigationPageStack("main", "Main");
stack.AddPage<RootPage>(String.Empty);
stack.AddPage<Page1>("page1");
```

Navigate by route:

```csharp
await Navigation.PushAsync("/main/page1");
```

## Requirements

- Avalonia 12
- .NET 10

RouteNav's navigation controls use Avalonia 12 primitives and resource keys where practical, so standard Fluent theming still applies. The sidebar/drawer layout composes Avalonia's native `DrawerPage`, and `NavigationControl` uses Avalonia 12's shared navigation-bar resource keys.

## Installation

```
dotnet add package RouteNav.Avalonia
```

## Documentation

Full documentation, migration notes and the demo app are available on GitHub: <https://github.com/profix898/RouteNav.Avalonia/blob/main/Docs/README.md>

Licensed under the MIT license.
