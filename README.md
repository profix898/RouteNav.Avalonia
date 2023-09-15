RouteNav.Avalonia
==========
[![Nuget](https://img.shields.io/nuget/v/RouteNav.Avalonia?style=flat-square&logo=nuget&color=blue)](https://www.nuget.org/packages/RouteNav.Avalonia)

**RouteNav.Avalonia** provides URI routing navigation for **Avalonia UI**. It supports a code-first, modular/extensible navigation approach with page (and dialog) primitives.

### Concept
In most applications the navigation system is *operated* from within, i.e. clicking a button or menu item triggers the UI to rearrange. It is usually the task of the button/menu handler to perform the desired UI changes. This approach is also used in most MVVM frameworks.

**RouteNav.Avalonia** on the other hand inverts that approach (inside-out). All navigation is handled via a central `Navigation` class. URI-based Navigation is invoked by calling `Navigation.PushAsync(Uri, NavigationTarget)`. The trigger source can still be a control (button, menu item, etc.), but it can also be an external event (e.g. URI activation).
The UI update happens implicitely based on the association of the (named) `NavigationStack` with a `NavigationContainer` (i.e. a window or navigation layout). Currently, there are four layouts available: `ContentPageStack` (single page), `NavigationPageStack` (mobile-like, navigation bar with back button), `TabbedPageStack` (TabControl), `SidebarMenuPageStack` (Hamburger menu, split pane).

**RouteNav.Avalonia** also supports navigation with modal dialogs (and message dialogs).

### Usage
During initialization all `Page`s are registered with the DI container of the navigation system (or manually with a custom DI container):
```CSharp
Navigation.UIPlatform.RegisterPage<Page1>();
```

We then create a `NavigationStack` and add pages with associated URI routes:

```CSharp
var stack = new NavigationPageStack("stackName", "Stack Label");
stack.AddPage<Page1>("page1");
```

Upon navigating to a route, i.e. `/stackName/page1` (refers to `Page1`), the page is requested from the DI container (incl. optional DI injection of all dependencies) and pushed to the `NavigationContainer` for display.

The `Page`s and `Dialog`s can be constructed via *XAML* or other means (i.e. code-first).

**Note:** For multi-window (desktop) plaforms, you can enforce single-window (via `Navigation.WindowManager.ForceSingleWindow`) and/or overlay dialogs (via `Navigation.WindowManager.ForceOverlayDialogs`). On mobile (Android/iOS) and Browser platforms requesting a new window always replaces the current `NavigationStack` and dialogs are always displayed in page overlay (`NavigationTarget.Dialog` and `NavigationTarget.Dialog` behaves the same then).

### Documentation
There is currently only very limited documentation (incl. API docs) available. Please refer to the *DemoApp* for preliminary instructions and usage examples.

### License
RouteNav.Avalonia is licensed under the terms of the MIT license (<http://opensource.org/licenses/MIT>, see LICENSE.txt).

### Disclaimer
RouteNav.Avalonia is in early stages of development and should be considered experimental.
