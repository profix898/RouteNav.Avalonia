using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace RouteNav.Avalonia.Internal;

internal static class ControlPropertiesExtensions
{
    public static void ClonePropertiesTo(this ContentControl controlSource, ContentControl controlTarget)
    {
        // ContentControl
        controlTarget.Content = controlSource.Content;
        controlTarget.ContentTemplate = controlSource.ContentTemplate;
        controlTarget.HorizontalContentAlignment = controlSource.HorizontalContentAlignment;
        controlTarget.VerticalContentAlignment = controlSource.VerticalContentAlignment;

        ClonePropertiesTo((TemplatedControl) controlSource, controlTarget);
    }

    public static void ClonePropertiesTo(this TemplatedControl controlSource, TemplatedControl controlTarget)
    {
        // TemplatedControl
        controlTarget.Background = controlSource.Background;
        controlTarget.BorderBrush = controlSource.BorderBrush;
        controlTarget.BorderThickness = controlSource.BorderThickness;
        controlTarget.CornerRadius = controlSource.CornerRadius;
        controlTarget.FontFamily = controlSource.FontFamily;
        controlTarget.FontSize = controlSource.FontSize;
        controlTarget.FontStyle = controlSource.FontStyle;
        controlTarget.FontWeight = controlSource.FontWeight;
        controlTarget.FontStretch = controlSource.FontStretch;
        controlTarget.Foreground = controlSource.Foreground;
        controlTarget.Padding = controlSource.Padding;

        ClonePropertiesTo((Control) controlSource, controlTarget);
    }

    public static void ClonePropertiesTo(this Control controlSource, Control controlTarget)
    {
        // StyledElement
        controlTarget.Name = controlSource.Name;
        controlTarget.Theme = controlSource.Theme;
        controlTarget.Styles.AddRange(controlSource.Styles);
        foreach (var resourceProvider in controlSource.Resources.MergedDictionaries)
            controlTarget.Resources.MergedDictionaries.Add(resourceProvider);
        foreach (var themeVariant in controlSource.Resources.ThemeDictionaries)
            controlTarget.Resources.ThemeDictionaries.Add(themeVariant);

        // Visual
        controlTarget.IsVisible = controlSource.IsVisible;
        controlTarget.Opacity = controlSource.Opacity;
        controlTarget.FlowDirection = controlSource.FlowDirection;

        // Layoutable
        controlTarget.Width = controlSource.Width;
        controlTarget.MinWidth = controlSource.MinWidth;
        controlTarget.MaxWidth = controlSource.MaxWidth;
        controlTarget.Height = controlSource.Height;
        controlTarget.MinHeight = controlSource.MinHeight;
        controlTarget.MaxHeight = controlSource.MaxHeight;
        controlTarget.Margin = controlSource.Margin;
        controlTarget.HorizontalAlignment = controlSource.HorizontalAlignment;
        controlTarget.VerticalAlignment = controlSource.VerticalAlignment;

        // Control
        controlTarget.FocusAdorner = controlSource.FocusAdorner;
        controlTarget.Tag = controlSource;
        controlTarget.ContextMenu = controlSource.ContextMenu;
        controlTarget.ContextFlyout = controlSource.ContextFlyout;
    }
}
