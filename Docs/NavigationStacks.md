# Navigation Stacks

A navigation stack maps routes to pages and owns the current navigation state for one layout. RouteNav ships several stack types for common application patterns.

## Common Stack API

All stacks implement `INavigationStack`.

Important members:

```csharp
string Name { get; }
string Title { get; }
Uri BaseUri { get; }
LazyValue<NavigationContainer> ContainerPage { get; }
LazyValue<Page> RootPage { get; }
IPageResolver? PageResolver { get; set; }
WindowFactory? WindowFactory { get; set; }
```

`WindowFactory` is optional. When set, it creates fresh window shells for this stack's own windows; otherwise RouteNav uses the application-wide factory supplied to `SetMainWindow`. Dialog windows always use the application-wide default factory.

Page registration:

```csharp
stack.AddPage<HomePage>(String.Empty);
stack.AddPage<DetailsPage>("details");
stack.AddPage("custom", uri => new CustomPage(uri));
```

Navigation:

```csharp
await stack.PushAsync("details");
await stack.PushAsync(new DetailsPage());
await stack.PopAsync();
await stack.PopToRootAsync();
```

Events:

```csharp
stack.PageNavigated += args => { /* args.From / args.To */ };
stack.DialogNavigated += args => { /* args.From / args.To */ };
stack.RouteNavigated += args => { /* args.From / args.To */ };
```

## ContentPageStack

`ContentPageStack` is the simplest stack. It shows one page at a time without navigation chrome or page history.

Use it when:

- You want route-driven content replacement.
- Your application provides its own shell/navigation UI.
- You do not need a back button or page stack.

Example:

```csharp
var stack = new ContentPageStack("content", "Content");
Navigation.UIPlatform.AddStack(stack);

stack.AddPage<HomePage>(String.Empty);
stack.AddPage<DetailsPage>("details");
```

Pushing a page clears the previous page from the page stack.

## NavigationPageStack

`NavigationPageStack` shows pages in a `NavigationControl` with a title bar and back button.

Use it when:

- You want mobile-style push/pop navigation.
- You want built-in navigation bar/back button behavior.
- You want `Page.Title` to appear in the navigation bar.

Example:

```csharp
var mainStack = new NavigationPageStack(Navigation.MainStackName, "My App");
Navigation.UIPlatform.AddStack(mainStack);

mainStack.AddPage<HomePage>(String.Empty);
mainStack.AddPage<DetailsPage>("details");
mainStack.AddPage<SettingsPage>("settings");
```

Navigate forward:

```csharp
await Navigation.PushAsync("main", "details");
```

Pop back:

```csharp
await Navigation.PopAsync();
```

The back button is enabled when the page stack can be popped. On the main stack, popping beyond the root keeps the root page active.

## TabbedPageStack

`TabbedPageStack` creates a `TabControl` where each registered page becomes a tab. The tab header comes from `Page.Title`.

Use it when:

- Your app has a small, stable set of peer pages.
- You want each route to be directly reachable as a tab.
- You do not need push/pop history inside the tab stack.

Example:

```csharp
var tabbedStack = new TabbedPageStack("tabbed", "Tabbed");
Navigation.UIPlatform.AddStack(tabbedStack);

tabbedStack.AddPage<TabbedRootPage>(String.Empty);
tabbedStack.AddPage<SummaryPage>("summary");
tabbedStack.AddPage<DetailsPage>("details");
```

Selecting a tab updates the current page on the stack. Navigating to a route selects the tab that hosts the matching page.

## SidebarMenuPageStack

`SidebarMenuPageStack` creates a drawer/sidebar navigation layout based on RouteNav's `SidebarMenu`, which composes Avalonia 12's native `DrawerPage`.

Use it when:

- You want drawer navigation with route-aware menu items.
- You want the menu to link both within the stack and across stacks.
- You want Avalonia 12 DrawerPage theming/adaptive behavior.

Example:

```csharp
var sidebarStack = new SidebarMenuPageStack("sidebar", "Sidebar");
Navigation.UIPlatform.AddStack(sidebarStack);

sidebarStack.AddMenuItem<SidebarRootPage>(String.Empty, "Home");
sidebarStack.AddMenuItem<OverviewPage>("overview", "Overview");
sidebarStack.AddMenuItem<SettingsPage>("settings", "Settings");
sidebarStack.AddMenuItem("/tabbed/summary", "Tabbed: Summary");
```

`AddMenuItem<TPage>` both adds a menu item and registers the page if the route belongs to the sidebar stack. If the route points to another stack, it only adds the menu item.

Display modes:

```csharp
sidebarStack.DisplayMode = SidebarMenu.DisplayModeEnum.Auto;
```

Options:

- `Auto` - inline when wide, overlay when narrow.
- `Inline` - locked side-by-side drawer.
- `Overlay` - overlay drawer.
- `CompactOverlay` - overlay drawer with compact rail.

## RouteEventStack

`RouteEventStack` does not manage page history. It exposes a `RouteEvent` and lets you handle route URIs manually.

Use it when:

- A route should trigger an action rather than map to a normal page.
- Routes are plugin-driven or discovered dynamically.
- You want to route to pages without registering them up front.

Example:

```csharp
var actions = new RouteEventStack("actions", uri =>
{
    if (uri.AbsolutePath.EndsWith("/about"))
        return new AboutPage();

    return null;
});

Navigation.UIPlatform.AddStack(actions);
```

When the event returns a page, RouteNav shows it as a dialog on the main stack. If it returns `null`, navigation falls back to the root page.

## Choosing a Stack

| Need | Stack |
|------|-------|
| Replace one content area | `ContentPageStack` |
| Push/pop history and back button | `NavigationPageStack` |
| Stable peer pages as tabs | `TabbedPageStack` |
| Drawer/sidebar menu | `SidebarMenuPageStack` |
| Action/plugin/custom route handling | `RouteEventStack` |

## Custom Containers

You can customize stack containers by deriving from the corresponding container type and placing the expected host control in your layout.

For navigation pages, derive from `NavigationPageContainer` and include a named `NavigationControl`:

```xml
<stackContainers:NavigationPageContainer xmlns="https://github.com/avaloniaui"
                                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                                         xmlns:stackContainers="clr-namespace:RouteNav.Avalonia.StackContainers;assembly=RouteNav.Avalonia"
                                         xmlns:controls="clr-namespace:RouteNav.Avalonia.Controls;assembly=RouteNav.Avalonia"
                                         x:Class="MyApp.AppShell">
    <DockPanel>
        <Menu DockPanel.Dock="Top" />
        <controls:NavigationControl x:Name="NavigationControl" />
    </DockPanel>
</stackContainers:NavigationPageContainer>
```

Then use the generic stack type:

```csharp
var stack = new NavigationPageStack<AppShell>("main", "My App");
```

Other container names:

- `NavigationPageContainer.NavigationControlName` defaults to `NavigationControl`.
- `SidebarMenuPageContainer.SidebarMenuName` defaults to `SidebarMenu`.
- `TabbedPageContainer.TabControlName` defaults to `TabControl`.
