# Controls

RouteNav.Avalonia includes controls and commands that turn UI interactions into route navigation.

## RouteButton

`RouteButton` derives from Avalonia `Button` and implements `IRouteItem`.

```xml
<routing:RouteButton RoutePath="/main/details" Content="Open details" />
<routing:RouteButton RoutePath="details" Content="Open details on current stack" />
```

Properties:

- `RouteUri` - the route URI to navigate to.
- `RoutePath` - convenience setter for relative or absolute route paths.
- `Target` - the `NavigationTarget` used for navigation.

Example with target:

```xml
<routing:RouteButton RoutePath="/main/details"
                     Target="Dialog"
                     Content="Open details dialog" />
```

## RouteMenuItem

`RouteMenuItem` is the menu equivalent of `RouteButton`.

```xml
<routing:RouteMenuItem Header="Settings" RoutePath="/main/settings" />
```

Use it in normal Avalonia menus when you want route navigation instead of custom click handlers.

## RouteCommand

`RouteCommand` implements `ICommand` and can be used from view models or command bindings.

```csharp
public ICommand OpenSettings { get; } = new RouteCommand
{
    RoutePath = "/main/settings",
    Target = NavigationTarget.Self
};
```

You can also create a command from any `IRouteItem`:

```csharp
var command = new RouteCommand(routeButton);
```

## HyperlinkButton and HyperlinkLabel

`HyperlinkButton` and `HyperlinkLabel` behave like hyperlinks. They inspect the target URI:

- Internal RouteNav URI -> `Navigation.PushAsync(...)`.
- External URI/file -> Avalonia's platform launcher.

```xml
<controls:HyperlinkButton NavigateUri="https://avaloniaui.net/"
                          Content="Avalonia" />

<controls:HyperlinkButton RoutePath="/main/details"
                          Content="Internal details link" />

<controls:HyperlinkLabel NavigateUri="https://avaloniaui.net/">
    Avalonia website
</controls:HyperlinkLabel>

<controls:HyperlinkLabel RoutePath="/main/details">
    Internal details link
</controls:HyperlinkLabel>
```

Both expose:

- `NavigateUri` / `RouteUri` - target URI.
- `RoutePath` - route-path convenience setter.
- `Target` - RouteNav navigation target.
- `IsVisited` - visited state.
- `TrackIsVisited` - sets visited state after successful launch/navigation.

## SidebarMenu

`SidebarMenu` is the control used by `SidebarMenuPageStack`. It composes Avalonia 12's native `DrawerPage` internally.

Typical apps do not create `SidebarMenu` directly; they create a `SidebarMenuPageStack` and add menu items:

```csharp
var sidebarStack = new SidebarMenuPageStack("sidebar", "Sidebar");
Navigation.UIPlatform.AddStack(sidebarStack);

sidebarStack.AddMenuItem<HomePage>(String.Empty, "Home");
sidebarStack.AddMenuItem<ReportsPage>("reports", "Reports");
sidebarStack.AddMenuItem("/tabbed/summary", "Tabbed: Summary");
```

Public members:

- `MenuItems` / `MenuItemsSource` - menu items shown in the drawer.
- `SelectedMenuItem` - selected item.
- `SelectedMenuItemChanged` - raised when selection changes.
- `Page` - content page hosted in the drawer content area.
- `DisplayMode` - drawer behavior.
- `InlineThresholdWidth` - breakpoint for `Auto` mode.
- `SidebarHeader` / `SidebarFooter` - drawer header/footer content.

Display modes:

```csharp
sidebarStack.DisplayMode = SidebarMenu.DisplayModeEnum.Auto;
```

`Auto` maps to Avalonia `DrawerBehavior.Auto` and switches between split/overlay behavior based on the threshold. `Inline`, `Overlay` and `CompactOverlay` map to the corresponding native drawer layouts.

## SidebarMenuItem

`SidebarMenuItem` carries the menu text and route.

```csharp
var item = new SidebarMenuItem
{
    Text = "Settings",
    RoutePath = "/main/settings",
    Target = NavigationTarget.Parent
};
```

For most apps, prefer `SidebarMenuPageStack.AddMenuItem(...)` because it adds the menu item and optionally registers the page in one call.

## Commands

RouteNav also ships simple command helpers:

```csharp
var command = new Command(() => Save());
var typedCommand = new Command<string>(value => Search(value));
```

Use these when you need a lightweight `ICommand` implementation independent of route navigation.
