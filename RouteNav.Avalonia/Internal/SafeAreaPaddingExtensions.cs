using Avalonia;
using static System.Math;

namespace RouteNav.Avalonia.Internal;

internal static class SafeAreaPaddingExtensions
{
    public static Thickness ApplySafeAreaPadding(this Thickness padding, Thickness safeAreaPadding)
    {
        return new Thickness(Max(padding.Left, safeAreaPadding.Left),
                             Max(padding.Top, safeAreaPadding.Top),
                             Max(padding.Right, safeAreaPadding.Right),
                             Max(padding.Bottom, safeAreaPadding.Bottom));
    }

    public static Thickness GetRemainingSafeAreaPadding(this Thickness padding, Thickness safeAreaPadding)
    {
        return new Thickness(Max(0, safeAreaPadding.Left - padding.Left),
                             Max(0, safeAreaPadding.Top - padding.Top),
                             Max(0, safeAreaPadding.Right - padding.Right),
                             Max(0, safeAreaPadding.Bottom - padding.Bottom));
    }
}
