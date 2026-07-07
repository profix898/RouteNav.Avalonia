# RouteNav.Avalonia

**RouteNav.Avalonia** provides URI-based navigation for **[Avalonia](https://avaloniaui.net/)**. It supports a code-first, modular/extensible navigation model built around page, dialog and stack primitives.

## Features

- Central URI navigation via `Navigation.PushAsync(uri, target)`
- Layout stacks for content pages, navigation-bar history, tabs and sidebar/drawer navigation
- Dialog, message-dialog and error-dialog support (desktop windows or overlays)
- Dependency-injection friendly page registration, custom factories and route resolvers
- Multi-window desktop and single-view mobile/browser support
- Avalonia 12 theming compatibility, including DrawerPage-based sidebar navigation

## Requirements

- Avalonia 12
- .NET 10

## Installation

```
dotnet add package RouteNav.Avalonia
```

Full documentation, migration notes and the multi-head demo app are available on GitHub:
<https://github.com/profix898/RouteNav.Avalonia>

Start with the repository `Docs/README.md` for the extended guides.

Upgrading from the Avalonia 11 build? See `Docs/Migration.md` in the repository.

Licensed under the MIT license.
