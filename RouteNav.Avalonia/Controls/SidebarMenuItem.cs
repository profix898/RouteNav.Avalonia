using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Controls;

[PseudoClasses(":pressed", ":selected")]
public class SidebarMenuItem : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<SidebarMenuItem, string>(nameof(PageType));

    public static readonly StyledProperty<Uri> RouteUriProperty = AvaloniaProperty.Register<SidebarMenuItem, Uri>(nameof(RouteUri));

    public static readonly StyledProperty<NavigationTarget> TargetProperty = AvaloniaProperty.Register<SidebarMenuItem, NavigationTarget>(nameof(Target), NavigationTarget.Self);

    public static readonly StyledProperty<Type?> PageTypeProperty = AvaloniaProperty.Register<SidebarMenuItem, Type?>(nameof(PageType));

    public static readonly StyledProperty<Func<Uri, Page>?> PageFactoryProperty = AvaloniaProperty.Register<SidebarMenuItem, Func<Uri, Page>?>(nameof(PageFactory));

    static SidebarMenuItem()
    {
        PressedMixin.Attach<SidebarMenuItem>();
        FocusableProperty.OverrideDefaultValue(typeof(SidebarMenuItem), true);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<SidebarMenuItem>(AutomationControlType.ListItem);
    }
    
    public string Text
    {
        get { return GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public Uri RouteUri
    {
        get { return GetValue(RouteUriProperty); }
        set { SetValue(RouteUriProperty, value); }
    }

    /// <summary>Set RouteUri via route path. Both relative paths (e.g. 'myPage' relative to current stack) and
    ///          absolute paths (e.g. '/myStack/myPage') are supported. The leading '/' denotes an absolute path.</summary>
    public string RoutePath
    {
        set { SetValue(RouteUriProperty, value.StartsWith("/") ? new Uri(Navigation.BaseRouteUri, value.TrimEnd('/')) : new Uri(value.TrimEnd('/'), UriKind.Relative)); }
    }

    public NavigationTarget Target
    {
        get { return GetValue(TargetProperty); }
        set { SetValue(TargetProperty, value); }
    }

    public Type? PageType
    {
        get { return GetValue(PageTypeProperty); }
        set
        {
            if (PageFactory != null)
                throw new ArgumentException($"Only one of {nameof(PageType)} or {nameof(PageFactory)} can be provided.");

            SetValue(PageTypeProperty, value);
        }
    }

    public Func<Uri, Page>? PageFactory
    {
        get { return GetValue(PageFactoryProperty); }
        set
        {
            if (PageType != null)
                throw new ArgumentException($"Only one of {nameof(PageType)} or {nameof(PageFactory)} can be provided.");

            SetValue(PageFactoryProperty, value);
        }
    }

    internal SidebarMenuItem Clone()
    {
        var menuItem = new SidebarMenuItem
        {
            Text = Text,
            RouteUri = RouteUri,
            Target = Target
        };
        if (PageType != null)
            menuItem.PageType = PageType;
        else
            menuItem.PageFactory = PageFactory;

        return menuItem;
    }
}
