RouteNav.Avalonia
==========
[![Nuget](https://img.shields.io/nuget/v/RouteNav.Avalonia?style=flat-square&logo=nuget&color=blue)](https://www.nuget.org/packages/RouteNav.Avalonia)

**RouteNav.Avalonia** provides URI-based navigation for **[Avalonia](https://avaloniaui.net/)**. It supports a code-first, modular/extensible navigation model built around page, dialog and stack primitives.

### Features

- **URI-based navigation** from the central `Navigation` facade — `Navigation.PushAsync(uri, target)`.
- **Four navigation layouts**: single page, navigation stack (back button), tabbed, and sidebar/drawer — the latter built on Avalonia 12's native `DrawerPage`.
- **Modal, message and error dialogs**, shown as windows (desktop) or overlays/embedded (mobile, browser); any `Page` can be shown as a dialog.
- **Cross-stack navigation** with explicit targets (`Self`, `Parent`, `Dialog`, `DialogOverlay`, `Window`).
- **Dependency-injection friendly** page registration (or custom page factories/resolvers).
- **Multi-window** (desktop) and **single-view** (mobile/browser) support behind one `Window` abstraction.
- **Routing controls**: `RouteButton`, `RouteMenuItem`, `RouteCommand`, `HyperlinkButton` and `HyperlinkLabel`.

### Concept
In most applications the navigation system is *operated* from within, i.e. clicking a button or menu item triggers the UI to rearrange. It is usually the task of the button/menu handler to perform the desired UI changes. This approach is also used in most MVVM frameworks.

**RouteNav.Avalonia** inverts that approach: navigation is addressed from the outside by URI. URI navigation is invoked by calling `Navigation.PushAsync(Uri, NavigationTarget)`. The trigger source can be a control (button, menu item, etc.), a command, or an external event such as URI activation. The UI update happens implicitly based on the association of the (named) `NavigationStack` with a `NavigationContainer` (i.e. a window or navigation layout). Currently, there are four layouts available: `ContentPageStack` (single page), `NavigationPageStack` (mobile-like, navigation bar with back button), `TabbedPageStack` (TabControl), `SidebarMenuPageStack` (hamburger/drawer menu, built on Avalonia's `DrawerPage`).

**RouteNav.Avalonia** also supports navigation with modal dialogs (and message dialogs).

### Requirements

- **Avalonia** 12.x
- **.NET** 10

RouteNav's navigation controls use Avalonia 12 primitives and resource keys where practical, so standard Fluent theming still applies. The sidebar/drawer layout composes Avalonia's native `DrawerPage`, and `NavigationControl` uses Avalonia 12's shared navigation-bar resource keys.

Upgrading from the Avalonia 11 build? See the [migration guide](Docs/Migration.md).

### Installation

Install the package from [NuGet](https://www.nuget.org/packages/RouteNav.Avalonia):

```
dotnet add package RouteNav.Avalonia
```

### Usage

During application initialization, register the `Page` types that should be resolved through the navigation DI container:
```CSharp
Navigation.UIPlatform.RegisterPage<RootPage, Page1>();
```

Then create a `NavigationStack` and add the individual pages with their associated routes:

```CSharp
var stack = new NavigationPageStack("stackName", "Stack Label");
stack.AddPage<RootPage>(String.Empty);
stack.AddPage<Page1>("page1");
...

// or using a page factory (for deferred or custom page initialization)
stack.AddPage("page1", uri => new Page1(uri));
```

Upon navigating to a route, e.g. `/stackName/page1` (which in this example maps to `Page1`), the page is requested from the DI container (including optional dependency injection) and pushed to the `NavigationContainer` for display. Each stack contains a root page (empty relative path), which is displayed as the default page for that stack.

The library provides `Page` and `Dialog` primitives, which enable construction of pages / dialogs via *XAML* or code. `Page`s constitute the main building blocks for content in **RouteNav.Avalonia**. Pages can also be converted for display in dialogs on the fly. In contrast, `Dialog`s are always shown in dialog windows (on desktop platforms), as overlays or embedded into a page.

**Note:** On multi-window desktop platforms, you can force single-window behavior via `Navigation.UIPlatform.WindowManager.ForceSingleWindow` and/or force overlay dialogs via `Navigation.UIPlatform.WindowManager.ForceOverlayDialogs`. On mobile (Android/iOS) and Browser platforms, requesting a new window falls back to replacing the current `NavigationStack`, and dialogs are displayed as overlays.

### Details & Advanced Usage

#### Window abstraction

**RouteNav.Avalonia** abstracts the window concept. In Avalonia, only desktop platforms use a classic `Window`, while mobile platforms use a view as the `TopLevel`. Supply a factory in `OnFrameworkInitializationCompleted`; RouteNav calls it for every main, secondary and dialog host, then places that fresh window directly in the platform window or view. Window-level XAML, resources and dynamic bindings therefore resolve normally without property cloning.

```CSharp
ApplicationLifetime.SetMainWindow(context => new MyWindow());
```

The default factory applies to every stack. A stack can select its own shell when it opens in a window or hosts a dialog:

```CSharp
sidebarStack.WindowFactory = context => new SidebarWindow();
```

Factories must return a new, unattached `RouteNav.Avalonia.Window` instance on every call. `WindowCreationContext.Kind`, `Stack`, `Owner`, `Content`, `Title` and `Icon` describe the request; RouteNav installs the supplied content, title and icon after construction.

#### BaseUri

All navigation operations are URI-based. In many cases, it is sufficient to specify the relative path to a page or a stack, e.g. `/stackName/page1`. Internally, all routes are resolved against a fully qualified base URI. The base URI can be customized via `Navigation.BaseRouteUri` (defaults to `https://avalonia.local/`), which is useful for URI activation/deep-link scenarios.

#### NavigationTarget

For most navigation operations, a navigation target can optionally be specified. This allows **RouteNav.Avalonia** to decide in which mode the specified URI should be presented. `NavigationTarget` specifies the preferred target for the route (note that it is possible for the implementation of the navigation stack to ignore this value). The following options are available as navigation targets:

```CSharp
public enum NavigationTarget
{
    Self,           /// Opens the route in the current context (i.e. the same stack container).
    Parent,         /// Opens the route in the parent context (i.e. the same window).
    Dialog,         /// Opens the route in a dialog (associated with the related stack).
    DialogOverlay,  /// Opens the route in an overlay dialog (on top of the related stack).
    Window          /// Opens the route in a new window (potentially switching to the related stack).
}
```

**Note:** For URI navigation via `Navigation.PushAsync(Uri routeUri, NavigationTarget target)`, the target always defaults to `Self`.

#### Complex container pages

When a new stack is loaded (by navigating to a page on that stack), the `Container` page is instantiated first. In the simplest case, a `NavigationContainer` (derived from `ContentControl`) is used directly. For more complex applications, however, it is often desirable for the `NavigationControl` to be embedded in an application-specific layout. An example would be a window with a menu or toolbar at the top and the control for navigation located below. In such a case, the container can be customized by deriving from `NavigationContainer` (or a derived container such as `NavigationPageContainer` / `TabbedPageContainer`) and placing the navigation host control at any desired location. For `NavigationPageContainer`, the `NavigationControl` only needs to be identified by name through `NavigationControlName` (`NavigationControl` is the default name):

```XML
/// public partial class DesktopContainer : NavigationPageContainer { }
<NavigationPageContainer xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         x:Class="DesktopContainer">
    <DockPanel>
        <!-- Menu / Toolbar / etc. -->
        <Menu x:Name="AppMenu" DockPanel.Dock="Top" />
        <!-- NavigationControl -->
        <NavigationControl x:Name="NavigationControl" />
    </DockPanel>
</NavigationPageContainer>
```

#### Custom page resolver

Generally, pages are registered in the `INavigationStack` with their route via `AddPage()` so that they can be retrieved later by route Uri. Where there is a large number of pages or if the pages are not known in advance, a custom `IPageResolver` can be provided via the `INavigationStack.PageResolver` property. The `IPageResolver` interface contains a single member `Page? ResolveRoute(Uri routeUri)`, which is used to find and instantiate the appropriate page for a given route Uri.

#### Page to Dialog

**RouteNav.Avalonia** utilizes automatic conversion of `Page` to `Dialog` in cases where a `Page` is invoked for display as a dialog (or overlay dialog). You can also explicitly perform the conversion by calling the `ToDialog(Layoutable? parent)` extension method for `Page`.

```CSharp
var dialog = new TestPage { DialogSizeHint = DialogSize.Small }.ToDialog(this);
```

A `Dialog` contains a few additional properties to control the title bar and size of the dialog. For auto-conversion, the `DialogSizeHint` property of `Page` affects the resulting dialog size (which is typically proportionally derived from the size of the parent).

#### Direct dialog usage

When `Navigation.PushAsync(Uri routeUri, NavigationTarget target)` is called with the target option `Dialog` (or `DialogOverlay`), the corresponding page is displayed as a dialog. It is also possible to display a dialog explicitly. The `Dialog` class contains the following methods for this purpose:

```CSharp
Task<object?> ShowDialog(Window? parentWindow)
// or
Task<object?> ShowDialog(Page? parentPage)
```

A `Dialog` should have a `Window` or a `Page` as a parent. If a dialog is closed via `Close(object? result)`, the dialog's `Result` property is set and the `ShowDialog()` task returns that result (or `null`, if the dialog is closed without a result).

#### Message Dialog / Error Dialog

**RouteNav.Avalonia** contains a simple message-box implementation represented by the `MessageDialog` type. In the simplest case, a text message can be displayed as follows:

```CSharp
var result = await MessageDialog.Create("Title", "Message Text", MessageDialogButtons.OkCancel).ShowDialog(this);
```

`MessageDialog` is also used for user-relevant error messages (*internal error* and *page not found*). Custom errors can be rendered using the static `Error` factory, e.g. `var page = Error.Page(string message, Exception ex)` or `await Error.ShowDialog(string message, Exception ex)`.

#### Specialized controls for URI Routing

A frequently used source for navigation events are `Button`s. Therefore, **RouteNav.Avalonia** offers a derived control, the `RouteButton`, for which a `RoutePath` (relative URI) or `RouteUri` (absolute URI) can be specified. Pressing the button navigates to the specified route. For `RoutePath`, both relative paths (e.g. `myPage` relative to current stack) and absolute paths (e.g. `/myStack/myPage` including a stack name) are supported. The leading `/` denotes the long variant with stack name.

```XML
<RouteButton RoutePath="/main/page1">To Page1 on Main stack</RouteButton>
<RouteButton RoutePath="page1">To Page1 on current stack</RouteButton>
```

The library also contains a `HyperlinkButton`, i.e. a `Button` control that functions as a navigable hyperlink, and a `HyperlinkLabel`, which is a `TextBlock` that functions as a navigable hyperlink. Both controls inspect the specified URI (`NavigateUri` / `RouteUri`): a valid internal route leads to RouteNav navigation, while other URIs are passed to Avalonia's standard launcher (e.g. to open a browser or trigger a platform URI activation).

```XML
<HyperlinkButton NavigateUri="https://www.avaloniaui.net/">External Link</HyperlinkButton>
<HyperlinkButton RoutePath="/main/page1">Internal Link (Page 1)</HyperlinkButton>
// and
<HyperlinkLabel NavigateUri="https://www.avaloniaui.net/">External Link</HyperlinkLabel>
<HyperlinkLabel RoutePath="/main/page1">Internal Link (Page 1)</HyperlinkLabel>
```

#### Sidebar Menu

The `SidebarMenuPageStack` is somewhat special: the menu can link to other stacks and external URIs (without having to leave the current stack). Where other stacks require `stack.AddPage()`, the sidebar menu is populated via `stack.AddMenuItem()` as shown below.

```CSharp
sidebarMenuStack.AddMenuItem<SidebarMenuPage1>("page1", "Page1");
sidebarMenuStack.AddMenuItem("/otherStack/pageX", "External Page"); // Links to a page on another stack
```

### Documentation
The repository includes XML API documentation, a multi-head *DemoApp*, and detailed guides in the [Docs](Docs/README.md) folder.

Recommended reading:

- [Getting Started](Docs/GettingStarted.md)
- [Concepts](Docs/Concepts.md)
- [Routing](Docs/Routing.md)
- [Navigation Stacks](Docs/NavigationStacks.md)
- [Dialogs and Errors](Docs/DialogsAndErrors.md)
- [Controls](Docs/Controls.md)
- [Theming and Layout](Docs/ThemingAndLayout.md)
- [Platform Notes](Docs/PlatformNotes.md)
- [Advanced Topics](Docs/AdvancedTopics.md)

The demo app contains examples for main-stack navigation, route parameters, dialogs, hyperlinks, sidebar/drawer navigation and tabbed navigation. Its navigation structure is defined in `DemoApp/DemoApp/App.axaml.cs`.

Upgrading from the Avalonia 11 build of RouteNav.Avalonia? See [Docs/Migration.md](Docs/Migration.md) for the breaking changes and step-by-step guidance.

### License
RouteNav.Avalonia is licensed under the terms of the MIT license (<http://opensource.org/licenses/MIT>, see LICENSE.txt).
