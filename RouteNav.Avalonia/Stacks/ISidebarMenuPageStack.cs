using System.Collections.Generic;
using RouteNav.Avalonia.Controls;

namespace RouteNav.Avalonia.Stacks;

/// <summary>A navigation stack whose pages are driven by a sidebar/drawer menu.</summary>
public interface ISidebarMenuPageStack : INavigationStack
{
    /// <summary>Gets the menu items that make up the sidebar.</summary>
    IReadOnlyList<SidebarMenuItem> MenuItems { get; }

    /// <summary>Adds a menu item to the sidebar.</summary>
    void AddMenuItem(SidebarMenuItem item);
}
