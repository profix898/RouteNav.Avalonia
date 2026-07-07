# RouteNav.Avalonia Documentation

Welcome to the extended documentation for **RouteNav.Avalonia**.

RouteNav.Avalonia is a URI-based navigation library for Avalonia. It provides a central navigation facade, multiple navigation stack layouts, dialog support, routing-aware controls, and a platform-neutral window abstraction for desktop, mobile and browser heads.

## Documentation Map

- [Getting Started](GettingStarted.md) - install the package, bootstrap RouteNav, register pages and perform the first navigation.
- [Concepts](Concepts.md) - understand routes, stacks, pages, containers, targets, windows and dialogs.
- [Routing](Routing.md) - build route URIs, use query parameters, navigate between stacks and handle route activation.
- [Navigation Stacks](NavigationStacks.md) - use `ContentPageStack`, `NavigationPageStack`, `TabbedPageStack`, `SidebarMenuPageStack` and `RouteEventStack`.
- [Dialogs and Errors](DialogsAndErrors.md) - show pages as dialogs, use `MessageDialog`, configure sizing and display error pages/dialogs.
- [Controls](Controls.md) - use `RouteButton`, `RouteMenuItem`, `RouteCommand`, `HyperlinkButton`, `HyperlinkLabel` and `SidebarMenu`.
- [Theming and Layout](ThemingAndLayout.md) - theme RouteNav controls, customize containers and work with Avalonia 12 navigation resources.
- [Platform Notes](PlatformNotes.md) - understand desktop, mobile, browser, Android activity lifetime and custom `IWindowManager` behavior.
- [Advanced Topics](AdvancedTopics.md) - custom page resolvers, custom stacks, custom error views and URI activation patterns.
- [Avalonia 11 to 12 Migration](Migration.md) - breaking changes and migration guidance.

## Requirements

- Avalonia 12.x
- .NET 10

## Demo App

The repository includes a multi-head demo app under `DemoApp/`. Its shared navigation setup is in `DemoApp/DemoApp/App.axaml.cs`. The demo covers:

- Main-stack navigation and back behavior.
- Manual route entry and query-parameter navigation.
- Dialogs, overlays, embedded dialogs and message dialogs.
- Built-in error pages.
- Hyperlink controls and direct page/dialog pushes.
- Sidebar/drawer navigation composed on Avalonia 12 `DrawerPage`.
- Tabbed navigation.

## API Documentation

The NuGet package includes XML API documentation for the public RouteNav API surface. Use these docs alongside IntelliSense and the guides in this folder.
