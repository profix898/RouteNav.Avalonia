using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Controls;

/// <summary>
/// A selectable item in a <see cref="SidebarMenu" />, carrying a display text and the route it navigates to.
/// </summary>
[PseudoClasses(":pressed", ":selected")]
public class SidebarMenuItem : TemplatedControl
{
    /// <summary>Defines the <see cref="Text" /> property.</summary>
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<SidebarMenuItem, string>(nameof(Text));

    /// <summary>Defines the <see cref="RouteUri" /> property.</summary>
    public static readonly StyledProperty<Uri> RouteUriProperty = AvaloniaProperty.Register<SidebarMenuItem, Uri>(nameof(RouteUri));

    /// <summary>Defines the <see cref="Target" /> property.</summary>
    public static readonly StyledProperty<NavigationTarget> TargetProperty = AvaloniaProperty.Register<SidebarMenuItem, NavigationTarget>(nameof(Target));

    static SidebarMenuItem()
    {
        PressedMixin.Attach<SidebarMenuItem>();
        FocusableProperty.OverrideDefaultValue(typeof(SidebarMenuItem), true);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<SidebarMenuItem>(AutomationControlType.ListItem);
    }

    /// <summary>Gets or sets the text shown for this menu item.</summary>
    public string Text
    {
        get { return GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    /// <summary>Gets or sets the route this menu item navigates to when selected.</summary>
    public Uri RouteUri
    {
        get { return GetValue(RouteUriProperty); }
        set { SetValue(RouteUriProperty, value); }
    }

    /// <summary>Set <see cref="RouteUri" /> via route path. Both relative paths (e.g. 'myPage' relative to current stack) and
    ///          absolute paths (e.g. '/myStack/myPage') are supported. The leading '/' denotes an absolute path.</summary>
    public string RoutePath
    {
        set { SetValue(RouteUriProperty, value.ParseRoutePath()); }
    }

    /// <summary>Gets or sets where the target route is shown when this item is selected.</summary>
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
