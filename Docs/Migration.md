# Migrating RouteNav.Avalonia from Avalonia 11 to Avalonia 12

This guide covers upgrading an application that uses **RouteNav.Avalonia** from the Avalonia 11 build of the library to the Avalonia 12 build.

The good news first: **RouteNav's core routing API is unchanged.** `Navigation.PushAsync(...)`, the navigation stacks (`ContentPageStack`, `NavigationPageStack`, `TabbedPageStack`, `SidebarMenuPageStack`), the `Page`/`Dialog` primitives, `RouteButton`, `HyperlinkButton`, `HyperlinkLabel`, `MessageDialog`, DI page registration and `Navigation.BaseRouteUri` all behave exactly as before. The main RouteNav-specific breaking changes are the window factory API, theming resource keys, a few `SidebarMenu` properties, and dev-tools wiring.

> RouteNav's Avalonia 12 build follows Avalonia's own breaking changes. Read the official [Avalonia 12 breaking changes](https://docs.avaloniaui.net/docs/avalonia12-breaking-changes) alongside this guide — anything you use directly from Avalonia still applies.

---

## At a glance

| Area | Avalonia 11 build | Avalonia 12 build |
|------|-------------------|-------------------|
| Target framework | `net8.0` / `net9.0` | `net10.0` (required for Android/iOS on Avalonia 12) |
| Avalonia | `11.3.x` | `12.0.x` |
| Dev tools | `Avalonia.Diagnostics` + per‑window `AttachDevTools()` | `AvaloniaUI.DiagnosticsSupport` + app‑level `AttachDeveloperTools()` |
| Window setup | Pass one `Window` instance | Pass a factory that creates a fresh `Window` per host |
| Nav bar theming | `NavigationControlNavigationBar*` keys | Avalonia's `NavigationBarBackground` / `NavigationBarForeground` |
| Sidebar/drawer | custom `SplitView`‑based control | composes Avalonia's native `DrawerPage` |

---

## 1. Update target frameworks and package references

Update **every** project in your solution — the app project and each platform head.

```diff
- <TargetFramework>net9.0</TargetFramework>
+ <TargetFramework>net10.0</TargetFramework>
```

```diff
- <PackageReference Include="Avalonia" Version="11.3.x" />
+ <PackageReference Include="Avalonia" Version="12.0.x" />
- <PackageReference Include="Avalonia.Desktop" Version="11.3.x" />
+ <PackageReference Include="Avalonia.Desktop" Version="12.0.x" />
- <PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.x" />
+ <PackageReference Include="Avalonia.Themes.Fluent" Version="12.0.x" />
```

Then update `RouteNav.Avalonia` to the Avalonia 12 build:

```
dotnet add package RouteNav.Avalonia
```

> **.NET 10 is required.** Avalonia 12 dropped .NET Framework/.NET Standard, and Android/iOS heads must target .NET 10 to match the underlying .NET SDK support.

---

## 2. Developer tools

Avalonia 12 replaced `Avalonia.Diagnostics` with `AvaloniaUI.DiagnosticsSupport`, and dev tools now attach at the **application** level (not per window). RouteNav no longer attaches dev tools to the windows it creates for you.

Project file (typically only in your app/shared project, `Debug` only):

```diff
- <PackageReference Condition="'$(Configuration)' == 'Debug'" Include="Avalonia.Diagnostics" Version="11.3.x" />
+ <PackageReference Condition="'$(Configuration)' == 'Debug'" Include="AvaloniaUI.DiagnosticsSupport" Version="2.2.0" />
```

Application code:

```diff
  public override void OnFrameworkInitializationCompleted()
  {
      base.OnFrameworkInitializationCompleted();

+ #if DEBUG
+     this.AttachDeveloperTools();
+ #endif

      ApplicationLifetime.SetMainWindow(_ => new MainWindow());
  }
```

Remove any `window.AttachDevTools()` calls — that API no longer exists.

### Window factory API

`SetMainWindow` now accepts a `WindowFactory`, not a window instance:

```diff
- ApplicationLifetime.SetMainWindow(new MainWindow());
+ ApplicationLifetime.SetMainWindow(context => new MainWindow());
```

The factory is the application default for main windows, secondary stack windows and dialog windows. It must return a new, unattached RouteNav window every time. RouteNav hosts that instance directly, so XAML resources and dynamic bindings resolve normally; the former property-cloning machinery has been removed.

Use a stack-specific factory when a stack needs its own shell:

```csharp
sidebarStack.WindowFactory = context => new SidebarWindow();
```

---

## 3. Navigation bar theming (`NavigationControl`)

`NavigationControl` now **consumes Avalonia 12's own navigation-bar resource keys** so that a single override restyles both the native `NavigationPage` and RouteNav's navigation bar. The RouteNav-specific keys were removed.

| Removed key (Avalonia 11 build) | Use instead (Avalonia 12) |
|---------------------------------|---------------------------|
| `NavigationControlNavigationBarBackground` | `NavigationBarBackground` |
| `NavigationControlNavigationBarTextColor` | `NavigationBarForeground` |

If you overrode the old keys, rename them:

```diff
- <SolidColorBrush x:Key="NavigationControlNavigationBarBackground" Color="#1565C0" />
- <SolidColorBrush x:Key="NavigationControlNavigationBarTextColor" Color="White" />
+ <SolidColorBrush x:Key="NavigationBarBackground" Color="#1565C0" />
+ <SolidColorBrush x:Key="NavigationBarForeground" Color="White" />
```

The `NavigationControl` control API is unchanged (`NavigationBarBackground`, `NavigationBarTextColor`, `NavigationBarVisible`, `BackButtonEnabled`, `Page`, `PageTransition`, `SafeAreaPadding`, and the `BackButtonClick` event).

---

## 4. Sidebar/drawer menu (`SidebarMenu`)

`SidebarMenu` now **composes Avalonia's native `DrawerPage`** internally instead of a custom `SplitView` template. The routing-facing API is preserved:

`MenuItems`, `MenuItemsSource`, `SelectedMenuItem`, `SelectedMenuItemChanged`, `Page`, `DisplayMode` (`Auto` / `Inline` / `Overlay` / `CompactOverlay`), `InlineThresholdWidth`, `SidebarHeader`, `SidebarFooter`, `SafeAreaPadding` and `NavigationStack` all work as before, and `SidebarMenuPageStack.AddMenuItem(...)` is unchanged.

### Removed properties

These belonged to the old `SplitView` implementation and were removed:

| Removed property | Replacement |
|------------------|-------------|
| `SidebarBackground` | Theme the drawer pane via the native `DrawerPage` (`DrawerBackground` / drawer Fluent keys) |
| `ContentBackground` | Set the page `Background` (or theme `DrawerPage`) |
| `HorizontalContentAlignment` / `VerticalContentAlignment` | Set alignment on the hosted page content |

### Theming

The drawer is now themed by Avalonia's `DrawerPage` control theme. RouteNav's old `SidebarMenu*` visual resource keys (pane/content backgrounds, hamburger, pane widths) no longer drive the visuals. To restyle, override the native `DrawerPage` theme or its Fluent resource keys, e.g. per instance:

```csharp
sidebarMenu.Resources["DrawerBackground"] = Brushes.WhiteSmoke;
```

Pane width and the compact rail are now controlled by `DrawerPage` (`DrawerLength` / `CompactDrawerLength`) rather than the former `SidebarMenuExpandWidth` / `SidebarMenuCompactWidth` keys. Menu items are still rendered as `SidebarMenuItem` and themed via `SidebarMenuItem.axaml` (the `SidebarMenuSelectionPipeFill` key still applies).

---

## 5. Platform heads

Avalonia 12 changed application initialization on some platforms.

- **Android:** the generic `AvaloniaMainActivity<TApp>` and its `CustomizeAppBuilder` override were removed. Change your `MainActivity` to inherit from the non-generic `AvaloniaMainActivity`, and add an `AvaloniaAndroidApplication<App>` class (marked `[Application]`). **Move your RouteNav setup** (`UseRouteNavUIPlatform(...)`) from the old activity's `CustomizeAppBuilder` into the new application class:

  ```csharp
  [Activity(Label = "MyApp.Android", MainLauncher = true, /* ... */)]
  public class MainActivity : AvaloniaMainActivity { }

  [Application]
  public class AndroidApp : AvaloniaAndroidApplication<App>
  {
      protected AndroidApp(nint javaReference, JniHandleOwnership transfer)
          : base(javaReference, transfer) { }

      protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
          base.CustomizeAppBuilder(builder)
              .UseRouteNavUIPlatform("https://myapp.local", /* DI provider */);
  }
  ```

  You do **not** need to hand-write `IActivityApplicationLifetime` / `MainViewFactory` handling — RouteNav's `ApplicationLifetime.SetMainWindow(...)` now recognizes `IActivityApplicationLifetime` (Avalonia 12's Android lifetime) and wires up the `MainViewFactory` for you.
- **iOS:** the `AvaloniaAppDelegate<App>` pattern is unchanged; iOS now uses the scene-based lifecycle and `AvaloniaAppDelegate.Window` stays `null` after startup.
- **Browser:** unchanged for RouteNav; ensure you reference `Avalonia.Browser` (the old Blazor backend was removed).

RouteNav's `Window` abstraction and `ApplicationLifetime.SetMainWindow(...)` continue to work across desktop, single-view (iOS/browser) and activity (Android) platforms.

See the [Avalonia 12 breaking changes](https://docs.avaloniaui.net/docs/avalonia12-breaking-changes) for the exact per-platform steps.

### Custom `IWindowManager` implementations

If you implement a custom `IWindowManager`, the `CreatePlatformView` signature was widened so it can serve both single-view and activity lifetimes:

```diff
- ContentControl CreatePlatformView(Window window, ISingleViewApplicationLifetime singleViewLifetime);
+ ContentControl CreatePlatformView(Window window, IApplicationLifetime appLifetime);
```

`CreatePlatformWindow` also receives the dialog role so custom managers can preserve the built-in customization contract:

```diff
- Avalonia.Controls.Window CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime lifetime);
+ Avalonia.Controls.Window CreatePlatformWindow(Window window, IClassicDesktopStyleApplicationLifetime lifetime, bool isDialogWindow = false);
```

Most apps use the built-in `AvaloniaWindowManager` and are unaffected.

---

## 6. Avalonia changes worth checking in RouteNav apps

These are Avalonia 12 changes (not RouteNav-specific) that commonly affect navigation apps:

- **Compiled bindings are on by default.** Fix any reflection-only bindings.
- **`TopLevel` is no longer necessarily the visual root.** Use `TopLevel.GetTopLevel(control)` instead of casting the root visual.
- **Gesture events moved to `InputElement`.** Drop the `Gestures.` prefix in XAML.
- **Focus event args** changed to `FocusChangedEventArgs` for `GotFocus`/`LostFocus`.
- **`Window.SystemDecorations` → `Window.WindowDecorations`** (and `ExtendClientAreaChromeHints` removed).
- **Clipboard/drag-drop** APIs were reworked (`IDataObject` removed).

---

## What you do **not** need to change

- `Navigation.PushAsync`, `PopAsync`, `EnterStack`, `BaseRouteUri`, `NavigationTarget`.
- Stack types and registration: `AddPage<T>()`, `AddPage(route, factory)`, `AddMenuItem(...)`, `IPageResolver`.
- `Page` / `Dialog` primitives, `Page.ToDialog(...)`, `DialogSizeHint`, `MessageDialog`, `Error.Page` / `Error.ShowDialog`.
- `RouteButton`, `RouteMenuItem`, `RouteCommand`, `HyperlinkButton`, `HyperlinkLabel`.
- Custom container pages (classes deriving from `NavigationPageContainer` / `TabbedPageContainer` / `SidebarMenuPageContainer`) and the `NavigationControlName` / `SidebarMenuName` / `TabControlName` scoped-control mechanism.

---

## Links

- [Avalonia 12 breaking changes](https://docs.avaloniaui.net/docs/avalonia12-breaking-changes)
- [RouteNav.Avalonia on NuGet](https://www.nuget.org/packages/RouteNav.Avalonia)
- [RouteNav.Avalonia on GitHub](https://github.com/profix898/RouteNav.Avalonia)
