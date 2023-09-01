using System.Collections.Generic;
using RouteNav.Avalonia.Controls;

namespace RouteNav.Avalonia.Stacks;

public interface ISidebarMenuPageStack
{
    IReadOnlyList<SidebarMenuItem> MenuItems { get; }

    void AddMenuItem(SidebarMenuItem item);
}
