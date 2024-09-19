RouteNav.Avalonia
==========
[![Nuget](https://img.shields.io/nuget/v/RouteNav.Avalonia?style=flat-square&logo=nuget&color=blue)](https://www.nuget.org/packages/RouteNav.Avalonia)

**RouteNav.Avalonia** provides URI routing navigation for **[Avalonia](https://avaloniaui.net/)**. It supports a code-first, modular/extensible navigation approach with page (and dialog) primitives.

### Concept
In most applications the navigation system is *operated* from within, i.e. clicking a button or menu item triggers the UI to rearrange. It is usually the task of the button/menu handler to perform the desired UI changes. This approach is also used in most MVVM frameworks.

**RouteNav.Avalonia** on the other hand inverts that approach (inside-out). All navigation is handled via a central `Navigation` class. URI-based Navigation is invoked by calling `Navigation.PushAsync(Uri, NavigationTarget)`. The trigger source can again be a control (button, menu item, etc.), but it can also be an external event (e.g. URI activation).
The UI update happens implicitly based on the association of the (named) `NavigationStack` with a `NavigationContainer` (i.e. a window or navigation layout). Currently, there are four layouts available: `ContentPageStack` (single page), `NavigationPageStack` (mobile-like, navigation bar with back button), `TabbedPageStack` (TabControl), `SidebarMenuPageStack` (Hamburger menu, split pane).

**RouteNav.Avalonia** also supports navigation with modal dialogs (and message dialogs).

### Usage

During initialization of the application all `Page`s are registered with the DI container of the navigation system (or manually with a custom DI container):
```CSharp
Navigation.UIPlatform.RegisterPage<RootPage, Page1>();
```

We then create a `NavigationStack` and add the individual pages with their associated URI routes:

```CSharp
var stack = new NavigationPageStack("stackName", "Stack Label");
stack.AddPage<RootPage>(String.Empty);
stack.AddPage<Page1>("page1");
...

// or using a page factory (for deferred or custom page initialization)
stack.AddPage("page1", uri => new Page1(uri));
```

Upon navigating to a route, i.e. `/stackName/page1` (which in this example maps to `Page1`), the page is requested from the DI container (incl. optional DI injection of all dependencies) and pushed to the `NavigationContainer` for display. Each stack contains a root page (with empty relative path), which is displayed as the default page for the stack.

The library provides `Page` and `Dialog` primitives, which enable construction of pages / dialogs via *XAML* or code. `Page`s constitute the main building blocks for content in **RouteNav.Avalonia**. Pages can also be converted for display in dialogs on the fly. In contrast, `Dialog`s are always shown in dialog windows (on desktop platforms), as overlays or embedded into a page.

**Note:** For multi-window (desktop) plaforms, you can enforce single-window (via `Navigation.WindowManager.ForceSingleWindow`) and/or overlay dialogs (via `Navigation.WindowManager.ForceOverlayDialogs`). On mobile (Android/iOS) and Browser platforms requesting a new window always replaces the current `NavigationStack` and dialogs are always displayed in page overlay (`NavigationTarget.Window` and `NavigationTarget.Dialog` behave the same then). On desktop platforms, multi-window and dialog windows are supported.

### Details & Advanced Usage

#### Window abstraction

**RouteNav.Avalonia** abstracts the window concept. In Avalonia, only the desktop platforms use a classic `Window`, while on mobile platforms a view is used as the `TopLevel`. **RouteNav.Avalonia** uses the abstracted window solely as a template for creating new top level windows or views (applying the colors, styles, etc. specified therein). The window template is simply set as follows (in `OnFrameworkInitializationCompleted` of the `Application`):

```CSharp
ApplicationLifetime.SetMainWindow(new MyWindow());
```

#### BaseUri

All navigation operations are URI-based. In many cases, it is sufficient to specify the relative path to a page or a stack, e.g. `/stackName/page1`. Internally, all URIs are stored fully qualified. The base URI for all routes can be customized via `Navigation.BaseRouteUri` (defaults to `https://avalonia.local/`). Especially, for URI activation events this allows a seamless integration with the URI schema of the application.

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

When a new stack is loaded (by navigating to a page on that stack), the `Container` page is instantiated first. In the simplest case, a `NavigationContainer` (derived from `ContentControl`) is used directly. For more complex applications, however, it is often desirable for the `NavigationControl` to be embedded in an application-specific layout. An example would be a window with a menu or toolbar at the top and the control for navigation located below.
In such a case, the container page can be constructed using a factory method. The freely designed page will then inherit a `NavigationContainer` (or derived control, e.g. `TabbedPageContainer`) and contain the `NavigationControl` at any desired location. The `NavigationControl` only needs to be identified by name to the `NavigationContainer` by settings its `NavigationControlName` property (`NavigationControl` is the default name):

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

**RouteNav.Avalonia** utilizes automatic conversion of `Page` to `Dialog` in cases where a `Page` is invoked for display as a dialog (or overlay dialog). You can also explicitely perform the conversion by calling the `ToDialog(Layoutable? parent)` extension method for `Page`.

```CSharp
var dialog = new TestPage { DialogSizeHint = DialogSize.Small }.ToDialog(this);
```

A `Dialog` contains a few additional properties to control the title bar and size of the dialog. For auto-conversion, the `DialogSizeHint` property of `Page` affects the resulting dialog size (which is typically proportionally derived from the size of the parent).

#### Direct dialog usage

When `Navigation.PushAsync(Uri routeUri, NavigationTarget target)` is called with the target option `Dialog` (or `OverlayDialog`), the corresponding page is displayed as a dialog. However, it is also possible to display a dialog explicitly. The `Dialog` class contains the following methods for this purpose:

```CSharp
Task<object?> ShowDialog(Window? parentWindow)
// or
Task<object?> ShowDialog(Page? parentPage)
```

A `Dialog` should have a `Window` or a `Page` as a parent. If a `Dialog` is closed via the `void Close(object? result)` method, the dialog's `object? Result` property is set, and the `ShowDialog()` call returns the result (or `null`, if the dialog is closed without result).

#### Message Dialog / Error Dialog

**RouteNav.Avalonia** contains a simple MessageBox implementation represented by the `MessageDialog` type. In the simplest case, a text message can be displayed as follows:

```CSharp
var result = await MessageDialog.Create("Title", "Message Text", MessageDialogButtons.OkCancel).ShowDialog(this);
```

`MessageDialog` is also used for the output of user-relevant error messages (*internal error* and *page not found*). Custom errors in the program execution can be rendered into corresponding error messages using the static class `Error`, e.g. `var page = Error.Page(string message, Exception ex)` or even `Error.ShowDialog(string message, Exception ex)`.

#### Specialized controls for URI Routing

A frequently used source for navigation events are `Button`s. Therefore, **RouteNav.Avalonia** offers a derived control, the `RouteButton`, for which a `RoutePath` (relative URI) or `RouteUri` (absolute URI) can be specified. Pressing the button navigates to the specified route. For `RoutePath`, both relative paths (e.g. `myPage` relative to current stack) and absolute paths (e.g. `/myStack/myPage` including a stack name) are supported. The leading `/` denotes the long variant with stack name.

```XML
<RouteButton RoutePath="/main/page1">To Page1 on Main stack</RouteButton>
<RouteButton RoutePath="page1">To Page1 on current stack</RouteButton>
```

The library also contains a `HyperlinkButton`, i.e. a `Button` control that functions as a navigable hyperlink, and a `HyperlinkLabel`, which is a `TextBlock` that functions as a navigable hyperlink. Both controls inspect the specified URI (`NavigateUri`): a valid internal route leads to URI navigation, other URIs are passed to the standard launcher (e.g. to be displayed in the browser or to trigger a URI activation on the platform).

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
There is currently only limited documentation (incl. API docs) available. Please refer to the *DemoApp* for preliminary instructions and usage examples. For starters, the navigation structure of the *DemoApp* is defined in ``App.axaml.cs``.

### License
RouteNav.Avalonia is licensed under the terms of the MIT license (<http://opensource.org/licenses/MIT>, see LICENSE.txt).
