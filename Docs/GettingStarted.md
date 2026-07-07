# Getting Started

This guide walks through the smallest useful RouteNav.Avalonia setup: install the package, bootstrap the UI platform, register pages, create a stack and navigate by URI.

## 1. Install

```bash
dotnet add package RouteNav.Avalonia
```

RouteNav.Avalonia targets Avalonia 12 and .NET 10.

## 2. Add Styles

Include RouteNav styles in your application styles. In most projects this goes in `App.axaml` alongside your Avalonia theme:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App">
  <Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://RouteNav.Avalonia/Themes/RouteNavStyles.axaml" />
  </Application.Styles>
</Application>
```

## 3. Bootstrap RouteNav

Use `UseRouteNavUIPlatform(...)` while building the Avalonia app. The base route URI is used to resolve absolute internal routes.

```csharp
public static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .UseRouteNavUIPlatform(
            "https://myapp.local/",
            () => Services.BuildServiceProvider(),
            Services);
```

If your app uses a single object that implements both `IServiceCollection` and `IServiceProvider`, you can use the generic overload:

```csharp
builder.UseRouteNavUIPlatform("https://myapp.local/", container);
```

If you provide a custom platform implementation, use:

```csharp
builder.UseRouteNavUIPlatform("https://myapp.local/", myUiPlatform);
```

## 4. Create the Main Window

RouteNav uses its own `RouteNav.Avalonia.Window` abstraction so the same code works for desktop windows, mobile single views and browser views.

```csharp
public override void OnFrameworkInitializationCompleted()
{
    base.OnFrameworkInitializationCompleted();

#if DEBUG
    this.AttachDeveloperTools();
#endif

    ApplicationLifetime.SetMainWindow(new MainWindow());
}
```

On desktop, this creates an Avalonia `Window`. On mobile/browser, it creates the appropriate top-level view. On Avalonia 12 Android, RouteNav wires the activity lifetime through `IActivityApplicationLifetime.MainViewFactory`.

## 5. Define Pages

A RouteNav page derives from `RouteNav.Avalonia.Page`.

```csharp
public partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        Title = "Home";
    }
}
```

Pages can be defined in XAML or code. Each page has:

- `Title` - used by navigation bars, tabs and dialogs.
- `PageQuery` - query-string parameters parsed from the route URI.
- `DialogSizeHint` - default size hint when shown as a dialog.
- `SafeAreaPadding` - propagated from platform safe-area insets.

## 6. Register Pages

Register page types during app initialization. This enables DI construction and query injection.

```csharp
Navigation.UIPlatform.RegisterPage<HomePage, DetailsPage, SettingsPage>();
```

You can also register pages directly with your DI container and then resolve them through a custom page factory or `IPageResolver`.

## 7. Create and Register a Stack

A stack maps URI routes to pages and owns the current navigation container.

```csharp
var mainStack = new NavigationPageStack(Navigation.MainStackName, "My App");
Navigation.UIPlatform.AddStack(mainStack);

mainStack.AddPage<HomePage>(String.Empty);
mainStack.AddPage<DetailsPage>("details");
mainStack.AddPage<SettingsPage>("settings");
```

The empty route (`String.Empty`) is the root page of the stack.

## 8. Navigate

Navigate with a URI:

```csharp
await Navigation.PushAsync(new Uri("https://myapp.local/main/details"));
```

Or use a stack name and relative route:

```csharp
await Navigation.PushAsync("main", "details");
```

Or use a routing control in XAML:

```xml
<routing:RouteButton RoutePath="/main/details" Content="Open details" />
```

## 9. Pass Query Parameters

Route query strings are parsed into `Page.PageQuery`.

```csharp
await Navigation.PushAsync(new Uri("https://myapp.local/main/details?id=42&mode=edit"));
```

```csharp
if (PageQuery.TryGetValue("id", out var id))
{
    // use id
}
```

RouteNav also adds a `routeUri` entry containing the full resolved route URI.

## 10. Show a Dialog

Any page can be displayed as a dialog:

```csharp
await Navigation.PushAsync(
    new Uri("https://myapp.local/main/details?id=42"),
    NavigationTarget.Dialog);
```

Or explicitly:

```csharp
var result = await MessageDialog
    .Create("Confirm", "Continue?", MessageDialogButtons.OkCancel)
    .ShowDialog(this);
```

## Next Steps

- Read [Concepts](Concepts.md) for the mental model.
- Read [Navigation Stacks](NavigationStacks.md) to choose the right layout.
- Read [Routing](Routing.md) for URI and target behavior.
