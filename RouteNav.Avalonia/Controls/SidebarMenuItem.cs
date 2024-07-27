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
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<SidebarMenuItem, string>(nameof(Text));

    public static readonly StyledProperty<Uri> RouteUriProperty = AvaloniaProperty.Register<SidebarMenuItem, Uri>(nameof(RouteUri));

    public static readonly StyledProperty<NavigationTarget> TargetProperty = AvaloniaProperty.Register<SidebarMenuItem, NavigationTarget>(nameof(Target), NavigationTarget.Self);

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

    /// <summary>Set <see cref="RouteUri"/> via route path. Both relative paths (e.g. 'myPage' relative to current stack) and
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

    internal SidebarMenuItem Clone()
    {
        return new SidebarMenuItem { Text = Text, RouteUri = RouteUri, Target = Target };
    }
}
