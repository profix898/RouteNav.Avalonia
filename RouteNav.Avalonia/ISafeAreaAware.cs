using Avalonia;

namespace RouteNav.Avalonia;

public interface ISafeAreaAware
{
    public Thickness SafeAreaPadding { get; set; }
}
