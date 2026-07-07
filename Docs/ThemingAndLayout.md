# Theming and Layout

RouteNav.Avalonia is designed to fit into Avalonia applications without replacing the app's theme system. The Avalonia 12 build uses native controls and shared resource keys where practical.

## Include RouteNav Styles

Add RouteNav styles after your Avalonia theme:

```xml
<Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://RouteNav.Avalonia/Themes/RouteNavStyles.axaml" />
</Application.Styles>
```

## NavigationControl Theming

`NavigationControl` is RouteNav's navigation-bar control used by `NavigationPageStack`.

It exposes direct properties:

- `NavigationBarBackground`
- `NavigationBarTextColor`
- `NavigationBarVisible`
- `BackButtonEnabled`
- `PageTransition`

In Avalonia 12, its default theme consumes shared navigation-bar resource keys:

```xml
<SolidColorBrush x:Key="NavigationBarBackground" Color="#1F2937" />
<SolidColorBrush x:Key="NavigationBarForeground" Color="White" />
```

This keeps RouteNav's navigation bar aligned with Avalonia 12 navigation styling.

## Sidebar / Drawer Theming

`SidebarMenu` composes Avalonia 12's native `DrawerPage`. This means drawer styling is controlled by Avalonia's drawer theme/resources instead of a separate custom split-view implementation.

Typical customization points:

```csharp
sidebarMenu.Resources["DrawerBackground"] = Brushes.WhiteSmoke;
```

Or override the `DrawerPage` control theme in application styles.

RouteNav-specific sidebar concepts remain route-focused:

- `SidebarHeader`
- `SidebarFooter`
- `MenuItems`
- `SelectedMenuItem`
- `DisplayMode`
- `InlineThresholdWidth`

## Sidebar Display Modes

`SidebarMenu.DisplayMode` controls how RouteNav maps to Avalonia drawer behavior.

| RouteNav mode | Behavior |
|---------------|----------|
| `Auto` | Split/inline above `InlineThresholdWidth`, overlay below it |
| `Inline` | Permanently visible drawer next to content |
| `Overlay` | Drawer overlays content |
| `CompactOverlay` | Drawer overlays content with a compact rail |

Example:

```csharp
var sidebarStack = new SidebarMenuPageStack("sidebar", "Sidebar")
{
    DisplayMode = SidebarMenu.DisplayModeEnum.Auto
};
```

## Custom Containers

RouteNav stack containers can be customized by deriving from the built-in container classes.

### NavigationPageContainer

Use this when you want a `NavigationControl` inside a custom shell.

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

```csharp
var mainStack = new NavigationPageStack<AppShell>("main", "My App");
```

The default `NavigationControlName` is `NavigationControl`.

### SidebarMenuPageContainer

Use this when you want a `SidebarMenu` inside a custom layout. The default `SidebarMenuName` is `SidebarMenu`.

```xml
<stackContainers:SidebarMenuPageContainer ...>
    <Grid>
        <controls:SidebarMenu x:Name="SidebarMenu" />
    </Grid>
</stackContainers:SidebarMenuPageContainer>
```

### TabbedPageContainer

Use this when you want a `TabControl` inside a custom layout. The default `TabControlName` is `TabControl`.

```xml
<stackContainers:TabbedPageContainer ...>
    <DockPanel>
        <TabControl x:Name="TabControl" />
    </DockPanel>
</stackContainers:TabbedPageContainer>
```

## Safe Areas

RouteNav propagates safe-area padding through `ISafeAreaAware`.

Relevant properties:

- `Page.SafeAreaPadding`
- `NavigationContainer.SafeAreaPadding`
- `NavigationControl.SafeAreaPadding`
- `SidebarMenu.SafeAreaPadding`

On platforms with safe areas (mobile devices, notches, system bars), containers listen to `TopLevel.InsetsManager.SafeAreaChanged` and pass remaining safe-area padding down to hosted content.

## Page Backgrounds and Content

RouteNav pages are `ContentControl`s. A page can host any Avalonia content:

```xml
<routeNav:Page xmlns="https://github.com/avaloniaui"
               xmlns:routeNav="clr-namespace:RouteNav.Avalonia;assembly=RouteNav.Avalonia"
               Title="Settings"
               Background="{DynamicResource DemoPageBackground}">
    <StackPanel Margin="24" Spacing="12">
        <TextBlock Text="Settings" FontSize="24" />
        <ToggleSwitch Content="Enable sync" />
    </StackPanel>
</routeNav:Page>
```

For content alignment, set alignment/margins on the content itself or on your custom container.

## Migrating Old Sidebar Styling

If you used the Avalonia 11 RouteNav sidebar resources, migrate to Avalonia 12 `DrawerPage` theming.

Removed old properties/resources include:

- `SidebarBackground`
- `ContentBackground`
- `HorizontalContentAlignment`
- `VerticalContentAlignment`
- old pane/content background keys
- old sidebar width keys

Use `DrawerPage` theme/resources for drawer visuals and page/container styles for content visuals.
