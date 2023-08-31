using Avalonia;

namespace RouteNav.Avalonia.Platform;

public interface ISafeAreaAware
{
    public Thickness SafeAreaPadding { get; set; }
}
