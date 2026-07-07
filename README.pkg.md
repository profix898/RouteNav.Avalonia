# RouteNav.Avalonia

**RouteNav.Avalonia** provides URI routing navigation for **[Avalonia](https://avaloniaui.net/)**. It supports a code-first, modular/extensible navigation approach with page (and dialog) primitives.

## Features

- Central, URI-based navigation via `Navigation.PushAsync(uri, target)`
- Multiple navigation layouts: single page, navigation stack (back button), tabbed, and sidebar/drawer menu
- Modal and message dialogs (dialog window, overlay, or embedded)
- Dependency-injection page registration
- Multi-window (desktop) and single-view (mobile/browser) support

## Requirements

- Avalonia 12
- .NET 10

## Installation

```
dotnet add package RouteNav.Avalonia
```

Full documentation, usage instructions and examples are available on GitHub:
<https://github.com/profix898/RouteNav.Avalonia>

Upgrading from the Avalonia 11 build? See the migration guide (MIGRATION.md) in the repository.

Licensed under the MIT license.
