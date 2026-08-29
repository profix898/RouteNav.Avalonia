using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

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

        ((TemplatedControl) controlSource).ClonePropertiesTo(controlTarget);
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

        ((Control) controlSource).ClonePropertiesTo(controlTarget);
    }

    public static void ClonePropertiesTo(this Control controlSource, Control controlTarget)
    {
        // StyledElement (note: Name is intentionally not cloned - clones are code-created hosts whose
        //         identity must not shadow the template window's XAML name)
        controlTarget.Theme = controlSource.Theme;
        CloneStylesTo(controlSource.Styles, controlTarget.Styles);
        CloneResourcesTo(controlSource.Resources, controlTarget.Resources);

        // Visual (note: IsVisible is intentionally not cloned - setting IsVisible on a Window calls
        //         Show() which marks the window as shown and breaks subsequent ShowDialog calls)
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

        // Control (note: ContextMenu/ContextFlyout are intentionally not cloned - they are interactive
        //         single-target instances: ContextMenu re-parents its single Popup between hosts and
        //         FlyoutBase.Target holds one placement target, so sharing breaks them)
        controlTarget.FocusAdorner = controlSource.FocusAdorner;
        controlTarget.Tag = controlSource;
    }

    /// <summary>
    /// Copies styles into per-target instances. Styles, nested style collections and includes are resource
    /// providers that can only be owned by a single host in Avalonia, so the source instances must never be
    /// shared between the template window and the cloned platform window.
    /// </summary>
    private static void CloneStylesTo(Styles source, Styles target)
    {
        foreach (var style in source)
        {
            if (CloneStyle(style) is { } clonedStyle)
                target.Add(clonedStyle);
        }
    }

    private static IStyle? CloneStyle(IStyle style)
    {
        switch (style)
        {
            case StyleInclude styleInclude when styleInclude.Source != null:
                // Absolute sources can be re-included safely (deferred loading is preserved). Relative sources
                // depend on the original XAML base uri, so the loaded styles are cloned instead.
                if (styleInclude.Source.IsAbsoluteUri)
                    return new StyleInclude((Uri?) null) { Source = styleInclude.Source };
                return CloneStyle(styleInclude.Loaded);

            case Styles styles:
            {
                var clonedStyles = new Styles();
                CloneStylesTo(styles, clonedStyles);
                CloneResourcesTo(styles.Resources, clonedStyles.Resources);
                return clonedStyles;
            }

            case Style sourceStyle:
            {
                var clonedStyle = new Style { Selector = sourceStyle.Selector };

                foreach (var setter in sourceStyle.Setters)
                    clonedStyle.Setters.Add(setter);
                foreach (var animation in sourceStyle.Animations)
                    clonedStyle.Animations.Add(animation);
                foreach (var childStyle in sourceStyle.Children)
                {
                    if (CloneStyle(childStyle) is { } clonedChild)
                        clonedStyle.Children.Add(clonedChild);
                }
                CloneResourcesTo(sourceStyle.Resources, clonedStyle.Resources);

                return clonedStyle;
            }

            default:
                return null;
        }
    }

    /// <summary>
    /// Copies a resource dictionary into per-target instances (direct entries, theme variant dictionaries and
    /// merged dictionaries). Resource providers can only be owned by a single host in Avalonia, so sharing the
    /// source instances between windows is not allowed.
    /// </summary>
    private static void CloneResourcesTo(IResourceDictionary source, IResourceDictionary target)
    {
        // Direct resource entries (resolved via TryGetValue so deferred content is materialized)
        foreach (var key in source.Keys)
        {
            if (source.TryGetValue(key, out var value))
                target[key] = value;
        }

        // Theme variant dictionaries
        foreach (var pair in source.ThemeDictionaries)
        {
            if (pair.Value is ResourceDictionary themeDictionary)
            {
                var clonedThemeDictionary = CloneDictionary(themeDictionary);
                if (clonedThemeDictionary is IThemeVariantProvider themeVariantProvider)
                    themeVariantProvider.Key = pair.Value.Key;
                target.ThemeDictionaries[pair.Key] = clonedThemeDictionary;
            }
        }

        // Merged dictionaries
        foreach (var mergedDictionary in source.MergedDictionaries)
        {
            if (CloneResourceProvider(mergedDictionary) is { } clonedProvider)
                target.MergedDictionaries.Add(clonedProvider);
        }
    }

    private static IResourceProvider? CloneResourceProvider(IResourceProvider resourceProvider)
    {
        switch (resourceProvider)
        {
            case MergeResourceInclude mergeResourceInclude when mergeResourceInclude.Source != null:
                // Absolute sources can be re-included safely (deferred loading is preserved). Relative sources
                // depend on the original XAML base uri, so the loaded dictionary is cloned instead.
                if (mergeResourceInclude.Source.IsAbsoluteUri)
                    return new MergeResourceInclude((Uri?) null) { Source = mergeResourceInclude.Source };
                return CloneResourceProvider(mergeResourceInclude.Loaded);

            case ResourceInclude resourceInclude when resourceInclude.Source != null:
                if (resourceInclude.Source.IsAbsoluteUri)
                    return new ResourceInclude((Uri?) null) { Source = resourceInclude.Source };
                return CloneResourceProvider(resourceInclude.Loaded);

            case ResourceDictionary dictionary:
                return CloneDictionary(dictionary);

            default:
                return null;
        }
    }

    private static ResourceDictionary CloneDictionary(ResourceDictionary source)
    {
        ResourceDictionary clonedDictionary;
        try
        {
            clonedDictionary = (ResourceDictionary) Activator.CreateInstance(source.GetType());
        }
        catch
        {
            clonedDictionary = new ResourceDictionary();
        }

        CloneResourcesTo(source, clonedDictionary);
        return clonedDictionary;
    }
}
