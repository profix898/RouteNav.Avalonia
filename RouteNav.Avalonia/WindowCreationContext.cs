using Avalonia.Controls;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia;

/// <summary>Describes why RouteNav needs a new window and what it will host.</summary>
public sealed class WindowCreationContext
{
    /// <summary>Initializes a new window creation context.</summary>
    public WindowCreationContext(WindowKind kind, INavigationStack? stack = null, Window? owner = null, object? content = null, string? title = null, WindowIcon? icon = null)
    {
        Kind = kind;
        Stack = stack;
        Owner = owner;
        Content = content;
        Title = title;
        Icon = icon;
    }

    /// <summary>Gets the role of the requested window.</summary>
    public WindowKind Kind { get; }

    /// <summary>Gets the navigation stack associated with the window, if any.</summary>
    public INavigationStack? Stack { get; }

    /// <summary>Gets a value indicating whether the window hosts the main navigation stack.</summary>
    public bool IsMainWindow => Stack?.IsMainStack ?? false;

    /// <summary>Gets the owning RouteNav window, if any.</summary>
    public Window? Owner { get; }

    /// <summary>Gets the content RouteNav will place in the new window.</summary>
    public object? Content { get; }

    /// <summary>Gets the requested title, if RouteNav supplies one.</summary>
    public string? Title { get; }

    /// <summary>Gets the requested icon, if RouteNav supplies one.</summary>
    public WindowIcon? Icon { get; }
}

/// <summary>Identifies the role of a RouteNav window.</summary>
public enum WindowKind
{
    /// <summary>A window hosting a navigation stack (main or secondary). Stack-specific shells are selected via <see cref="Stacks.INavigationStack.WindowFactory" />, stack-specific behavior via <see cref="WindowCreationContext.IsMainWindow" />.</summary>
    Window,

    /// <summary>A modal window hosting a dialog.</summary>
    Dialog
}

/// <summary>Creates a fresh, unattached RouteNav window for the supplied context.</summary>
public delegate Window WindowFactory(WindowCreationContext context);
