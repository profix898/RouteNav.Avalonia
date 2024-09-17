using Avalonia;

namespace RouteNav.Avalonia.Platform;

/// <summary>Interface representing an entity that supports safe area padding.</summary>
public interface ISafeAreaAware
{
    /// <summary>
    /// Gets or sets the padding that accounts for the safe area.
    /// </summary>
    Thickness SafeAreaPadding { get; set; }
}
